using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using UAlbion.Api;
using UAlbion.Api.Eventing;
using UAlbion.Base;
using UAlbion.Formats.Assets.Inv;
using UAlbion.Formats.Assets.Save;
using UAlbion.Formats.Assets.Sheets;
using UAlbion.Formats.Ids;
using UAlbion.Game.State;
using UAlbion.Game.Combat;

namespace UAlbion.Tools.AgentBridge;

/// <summary>
/// Embedded WebSocket server that lets an external agent observe and control the game.
///
/// Endpoints:
///   ws://localhost:7399/agent  — bidirectional: send JSON commands, receive JSON responses
///   ws://localhost:7399/events — broadcast: all commands/responses + game events (read-only)
///   http://localhost:7399/monitor — browser dashboard
/// </summary>
public class AgentBridgeComponent : Component, IDisposable
{
    const int Port = 7399;

    readonly HttpListener _http = new();
    readonly ConcurrentQueue<(string cmd, TaskCompletionSource<string> tcs)> _commandQueue = new();
    readonly List<WebSocket> _eventClients = [];
    readonly Lock _eventLock = new();
    CancellationTokenSource _cts = new();

    static readonly string ScriptDir = Path.Combine(
        AppContext.BaseDirectory, "..", "..", "..", "..", "Tools", "AgentBridge", "test");

    public void Dispose()
    {
        _cts.Dispose();
        _http.Close();
        GC.SuppressFinalize(this);
    }

    public AgentBridgeComponent()
    {
        On<BeginFrameEvent>(_ => DrainCommandQueue());
        On<LogEvent>(BroadcastLog);
    }

    void BroadcastLog(LogEvent e)
    {
        var payload = JsonSerializer.Serialize(new { type = "log", severity = e.Severity.ToString(), message = e.Message });
        BroadcastEvent(payload);
    }

    protected override void Subscribed()
    {
        _cts = new CancellationTokenSource();
        _http.Prefixes.Add($"http://localhost:{Port}/");
        try
        {
            _http.Start();
        }
        catch (HttpListenerException ex)
        {
            Console.Error.WriteLine($"[AgentBridge] Failed to start HTTP listener on port {Port}: {ex.Message}");
            Console.Error.WriteLine("[AgentBridge] Try: netsh http add urlacl url=http://localhost:7399/ user=Everyone");
            return;
        }

        Console.WriteLine($"[AgentBridge] ws://localhost:{Port}/agent  ws://localhost:{Port}/events");
        Console.WriteLine($"[AgentBridge] Monitor: http://localhost:{Port}/monitor");
        Task.Run(() => AcceptLoopAsync(_cts.Token));

        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = $"http://localhost:{Port}/monitor",
                UseShellExecute = true
            });
        }
        catch { }
    }

    protected override void Unsubscribed()
    {
        _cts.Cancel();
        _http.Stop();
    }

    async Task AcceptLoopAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            HttpListenerContext ctx;
            try { ctx = await _http.GetContextAsync().WaitAsync(ct); }
            catch (OperationCanceledException) { break; }
            catch (Exception ex) { Console.Error.WriteLine($"[AgentBridge] Accept error: {ex.Message}"); break; }

            if (!ctx.Request.IsWebSocketRequest)
            {
                if (ctx.Request.HttpMethod == "GET" && ctx.Request.Url?.AbsolutePath == "/monitor")
                    _ = Task.Run(() => ServeMonitorAsync(ctx), ct);
                else if (ctx.Request.HttpMethod == "GET" && ctx.Request.Url?.AbsolutePath == "/scripts")
                    _ = Task.Run(() => ServeScriptListAsync(ctx), ct);
                else if (ctx.Request.HttpMethod == "POST" && ctx.Request.Url?.AbsolutePath == "/run-script")
                    _ = Task.Run(() => RunScriptAsync(ctx), ct);
                else
                {
                    ctx.Response.StatusCode = 400;
                    ctx.Response.Close();
                }
                continue;
            }

            _ = Task.Run(() => HandleWebSocketAsync(ctx, ct), ct);
        }
    }

    static async Task ServeMonitorAsync(HttpListenerContext ctx)
    {
        var html = Encoding.UTF8.GetBytes(MonitorHtml);
        ctx.Response.ContentType = "text/html; charset=utf-8";
        ctx.Response.ContentLength64 = html.Length;
        try { await ctx.Response.OutputStream.WriteAsync(html); ctx.Response.OutputStream.Close(); }
        catch { }
    }

    static async Task ServeScriptListAsync(HttpListenerContext ctx)
    {
        var scripts = Directory.Exists(ScriptDir)
            ? Directory.GetFiles(ScriptDir, "*.py").Select(Path.GetFileName).ToArray()
            : Array.Empty<string>();
        var json = JsonSerializer.Serialize(scripts);
        ctx.Response.ContentType = "application/json";
        var bytes = Encoding.UTF8.GetBytes(json);
        ctx.Response.ContentLength64 = bytes.Length;
        await ctx.Response.OutputStream.WriteAsync(bytes);
        ctx.Response.OutputStream.Close();
    }

    void RunScriptAsync(HttpListenerContext ctx)
    {
        var file = ctx.Request.QueryString["file"];
        ctx.Response.StatusCode = 202;
        ctx.Response.Close();
        if (string.IsNullOrEmpty(file)) return;
        var path = Path.Combine(ScriptDir, Path.GetFileName(file));
        _ = Task.Run(async () =>
        {
            using var proc = new System.Diagnostics.Process();
            proc.StartInfo = new System.Diagnostics.ProcessStartInfo("python", $"\"{path}\"")
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false
            };
            try { proc.Start(); }
            catch (Exception ex) { BroadcastEvent(JsonSerializer.Serialize(new { type = "script_error", file, message = ex.Message })); return; }
            async Task Relay(StreamReader reader)
            {
                string line;
                while ((line = await reader.ReadLineAsync()) != null)
                    BroadcastEvent(JsonSerializer.Serialize(new { type = "script_output", file, line }));
            }
            await Task.WhenAll(Relay(proc.StandardOutput), Relay(proc.StandardError));
            await proc.WaitForExitAsync();
            BroadcastEvent(JsonSerializer.Serialize(new { type = "script_done", file, exit = proc.ExitCode }));
        });
    }

    async Task HandleWebSocketAsync(HttpListenerContext ctx, CancellationToken ct)
    {
        var path = ctx.Request.Url?.AbsolutePath ?? "/";
        WebSocketContext wsCtx;
        try { wsCtx = await ctx.AcceptWebSocketAsync(null); }
        catch (Exception ex) { Console.Error.WriteLine($"[AgentBridge] WS handshake failed: {ex.Message}"); return; }

        var ws = wsCtx.WebSocket;

        if (path == "/events")
        {
            lock (_eventLock) _eventClients.Add(ws);
            await DrainUntilClosedAsync(ws, ct);
            lock (_eventLock) _eventClients.Remove(ws);
            return;
        }

        // /agent — bidirectional command/response
        var buf = new byte[65536];
        while (ws.State == WebSocketState.Open && !ct.IsCancellationRequested)
        {
            WebSocketReceiveResult result;
            try { result = await ws.ReceiveAsync(buf, ct); }
            catch { break; }

            if (result.MessageType == WebSocketMessageType.Close)
            {
                await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "", ct);
                break;
            }

            var text = Encoding.UTF8.GetString(buf, 0, result.Count);
            BroadcastEvent(JsonSerializer.Serialize(new { type = "command", payload = text }));

            var tcs = new TaskCompletionSource<string>();
            _commandQueue.Enqueue((text, tcs));

            string response;
            try { response = await tcs.Task.WaitAsync(TimeSpan.FromSeconds(10), ct); }
            catch (TimeoutException) { response = Error("timeout", "Game did not process command within 10s"); }
            catch (Exception ex) { response = Error("exception", ex.Message); }

            BroadcastEvent(JsonSerializer.Serialize(new { type = "response", payload = response }));

            var responseBytes = Encoding.UTF8.GetBytes(response);
            try { await ws.SendAsync(responseBytes, WebSocketMessageType.Text, true, ct); }
            catch { break; }
        }
    }

    static async Task DrainUntilClosedAsync(WebSocket ws, CancellationToken ct)
    {
        var buf = new byte[256];
        while (ws.State == WebSocketState.Open && !ct.IsCancellationRequested)
        {
            WebSocketReceiveResult result;
            try { result = await ws.ReceiveAsync(buf, ct); }
            catch { break; }
            if (result.MessageType == WebSocketMessageType.Close)
            {
                try { await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "", ct); } catch { }
                break;
            }
        }
    }

    void DrainCommandQueue()
    {
        while (_commandQueue.TryDequeue(out var item))
        {
            var (cmd, tcs) = item;
            string response;
            try { response = ExecuteCommand(cmd); }
            catch (Exception ex) { response = Error("exception", ex.Message); }
            tcs.SetResult(response);
        }
    }

    string ExecuteCommand(string raw)
    {
        JsonNode? node;
        try { node = JsonNode.Parse(raw); }
        catch { return Error("parse_error", "Invalid JSON"); }

        var cmd = node?["cmd"]?.GetValue<string>() ?? node?["command"]?.GetValue<string>() ?? "";

        return cmd switch
        {
            "ping"                   => Ok(new { pong = true }),
            "quit"                   => RaiseGameEvent("quit"),
            "get_scene"              => GetScene(),
            "get_party"              => GetParty(),
            "get_inventory"          => GetInventory(),
            "get_map"                => GetMap(),
            "get_time"               => GetTime(),
            "get_npcs"               => GetNpcs(),
            "get_combat"             => GetCombat(),
            "select_combat_action"   => SelectCombatAction(node),
            "select_combat_target"   => SelectCombatTarget(node),
            "equip_item"             => EquipItem(node),
            "save_game"              => SaveGame(node),
            "load_game"              => LoadGame(node),
            "teleport"               => Teleport(node),
            "load_map"               => LoadMap(node),
            "talk_npc"        => TalkNpc(node),
            "respond"         => Respond(node),
            "modify_gold"     => ModifyGold(node),
            "raise_event"     => RaiseGameEvent(node?["event"]?.GetValue<string>()),
            "start_new_game"  => RaiseGameEvent("new_game Map.TorontoBegin 31 76"),
            "dismiss_message" => RaiseGameEvent("dismiss_message"),
            _ => Error("unknown_command",
                $"Unknown command '{cmd}'. Available: ping, quit, get_scene, get_party, get_inventory, get_map, get_time, get_npcs, " +
                "get_combat, select_combat_action, select_combat_target, equip_item, save_game, load_game, teleport, load_map, " +
                "talk_npc, respond, modify_gold, raise_event, start_new_game, dismiss_message")
        };
    }

    // ── Read commands ─────────────────────────────────────────────────────────

    string GetScene()
    {
        var sm = TryResolve<ISceneManager>();
        if (sm == null)
            return Error("not_ready", "Scene manager not available.");
        return Ok(new { scene_id = sm.ActiveSceneId.ToString() });
    }

    string GetMap()
    {
        var state = TryResolve<IGameState>();
        if (state == null || !state.Loaded)
            return Error("not_ready", "No game loaded.");
        return Ok(new { map_id = state.MapId.ToString() });
    }

    string GetTime()
    {
        var state = TryResolve<IGameState>();
        if (state == null || !state.Loaded)
            return Error("not_ready", "No game loaded.");
        var t = state.Time;
        return Ok(new { day = t.Day, hour = t.Hour, minute = t.Minute, tick = state.TickCount });
    }

    string GetNpcs()
    {
        var state = TryResolve<IGameState>();
        if (state == null || !state.Loaded)
            return Error("not_ready", "No game loaded.");

        var npcs = new List<object>();
        foreach (var npc in state.Npcs)
        {
            npcs.Add(new
            {
                id = npc.Id.ToString(),
                type = npc.Type.ToString(),
                x = npc.X,
                y = npc.Y,
                movement = npc.MovementType.ToString()
            });
        }
        return Ok(new { map_id = state.MapId.ToString(), npcs });
    }

    string GetCombat()
    {
        var sm = TryResolve<ISceneManager>();
        var state = TryResolve<IGameState>();
        if (state == null)
            return Error("not_ready", "Game state not available");

        var sceneId = sm?.ActiveSceneId.ToString() ?? "unknown";
        
        var battle = TryResolve<IReadOnlyBattle>();
        int tileCount = SavedGame.CombatRows * SavedGame.CombatColumns;
        var tileMap = battle == null ? null :
            Enumerable.Range(0, tileCount).Select(i =>
            {
                var mob = battle.GetTile(i);
                return mob == null ? null : (object)new
                {
                    tile = i,
                    id = mob.SheetId.ToString(),
                    name = mob.Effective.GetName(Base.Language.English),
                    type = mob.Effective.Type.ToString(),
                    hp = mob.Effective.Combat.LifePoints.Current,
                    max_hp = mob.Effective.Combat.LifePoints.Max
                };
            }).ToList();

        var partyInCombat = new List<object>();
        foreach (var player in state.Party.WalkOrder)
        {
            var combatPos = state.GetCombatPositionForPlayer(player.Id);
            if (combatPos.HasValue)
            {
                partyInCombat.Add(new
                {
                    id = player.Id.ToString(),
                    position = combatPos.Value,
                    hp = player.Apparent?.Combat.LifePoints.Current ?? 0,
                    max_hp = player.Apparent?.Combat.LifePoints.Max ?? 0
                });
            }
        }

        return Ok(new { scene = sceneId, party = partyInCombat, tile_map = tileMap });
    }

    string SelectCombatAction(JsonNode node)
    {
        var battle = TryResolve<IReadOnlyBattle>();
        if (battle == null)
            return Error("not_in_combat", "Not in combat");

        if (node == null)
            return Error("missing_params", "Requires 'actor' (party member index 1-based) and 'action' (attack/move/flee/wait)");

        int actorIndex = node["actor"]?.GetValue<int>() ?? 0;
        string actionStr = node["action"]?.GetValue<string>() ?? "";

        var state = TryResolve<IGameState>();
        if (state == null)
            return Error("not_ready", "Game state not available");

        if (actorIndex < 1 || actorIndex > state.Party.WalkOrder.Count)
            return Error("invalid_actor", $"Actor index {actorIndex} out of range (1-{state.Party.WalkOrder.Count})");

        var player = state.Party.WalkOrder[actorIndex - 1];
        var combatPos = state.GetCombatPositionForPlayer(player.Id);
        if (!combatPos.HasValue)
            return Error("not_in_combat", $"Player {player.Id} is not in combat");

        if (!Enum.TryParse<CombatActionType>(actionStr, true, out var action))
            return Error("invalid_action", $"Unknown action '{actionStr}'. Use: Attack, Move, Flee, None");

        // Use RaiseGameEvent to ensure the event reaches Battle through the log exchange
        return RaiseGameEvent($"select_combat_action {combatPos.Value} {action}");
    }

    string SelectCombatTarget(JsonNode node)
    {
        var battle = TryResolve<IReadOnlyBattle>();
        if (battle == null)
            return Error("not_in_combat", "Not in combat");

        if (node == null)
            return Error("missing_params", "Requires 'target_tile' (tile index to target)");

        int targetTile = node["target_tile"]?.GetValue<int>() ?? -1;
        if (targetTile < 0)
            return Error("invalid_target", "target_tile must be >= 0");

        return RaiseGameEvent($"select_combat_target {targetTile}");
    }

    string GetInventory()
    {
        var state = TryResolve<IGameState>();
        if (state == null || !state.Loaded)
            return Error("not_ready", "No game loaded. Start a new game or load a save first.");

        var party = state.Party;
        var members = new List<object>();

        foreach (var player in party.WalkOrder)
        {
            var invId = new InventoryId(player.Id);
            var inv = state.GetInventory(invId);
            var slots = new List<object>();

            if (inv != null)
            {
                var allSlots = inv.Slots;
                for (int i = 0; i < allSlots.Count; i++)
                {
                    var slot = allSlots[i];
                    if (slot.Item.IsNone) continue;
                    slots.Add(new
                    {
                        slot = ((ItemSlotId)i).ToString(),
                        item = slot.Item.ToString(),
                        amount = slot.Amount
                    });
                }
            }

            members.Add(new
            {
                id = player.Id.ToString(),
                name = GetCharacterName(player.Apparent, player.Id.ToString()),
                gold = inv?.Gold?.Amount ?? 0,
                rations = inv?.Rations?.Amount ?? 0,
                slots
            });
        }

        return Ok(new { party_gold = party.TotalGold, members });
    }

    string GetParty()
    {
        var state = TryResolve<IGameState>();
        if (state == null || !state.Loaded)
            return Error("not_ready", "No game loaded.");

        var party = state.Party;
        var members = new List<object>();

        foreach (var player in party.WalkOrder)
        {
            var sheet = player.Apparent;
            members.Add(new
            {
                id = player.Id.ToString(),
                name = GetCharacterName(sheet, player.Id.ToString()),
                hp = sheet?.Combat.LifePoints.Current ?? 0,
                max_hp = sheet?.Combat.LifePoints.Max ?? 0,
                level = sheet?.Level ?? 0,
                class_name = sheet?.PlayerClass.ToString() ?? "",
                experience_points = sheet?.Combat.ExperiencePoints ?? 0,
                combat_position = player.CombatPosition
            });
        }

        return Ok(new { leader = party.Leader?.Id.ToString(), gold = party.TotalGold, members });
    }

// ── Action commands ───────────────────────────────────────────────────────

    string EquipItem(JsonNode? node)
    {
        var member = node?["member"]?.GetValue<string>();
        var item = node?["item"]?.GetValue<string>();
        var slot = node?["slot"]?.GetValue<string>() ?? "RightHand";
        
        if (member == null || item == null)
            return Error("missing_param", "Fields 'member' and 'item' are required");

        var state = TryResolve<IGameState>();
        if (state == null || !state.Loaded)
            return Error("not_ready", "No game loaded.");

        try
        {
            // Get party member directly from Base class
            PartyMemberId partyMemberId;
            try { partyMemberId = Base.PartyMember.Tom; }
            catch { partyMemberId = new PartyMemberId(1); } // Fallback: ID 1 for Tom
            
            var invId = new InventoryId(partyMemberId);
            var inv = ((GameState)state).GetWriteableInventory(invId);
            if (inv == null)
                return Error("not_found", $"Inventory not found for member");

            if (inv is not Inventory writableInv)
                return Error("not_supported", $"Inventory type {inv.GetType()} is not writable");

            // Get item directly from Base class
            ItemId itemId;
            try { itemId = Base.Item.Sword; }
            catch { itemId = new ItemId(0); } // Fallback: ID 0

            // Parse slot
            var slotId = ParseItemSlotId(slot);
            
            var itemSlot = writableInv.Slots[(int)slotId];
            itemSlot.Item = itemId;
            itemSlot.Amount = 1;
            
            // Trigger effective sheet recalculation so DisplayDamage updates
            var stateObj = (GameState)state;
            foreach (var m in stateObj.Party.WalkOrder)
            {
                member.GetType().GetMethod("UpdateSheet", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                    ?.Invoke(member, null);
            }
            
            return Ok(new { equipped = item, to = slot });
        }
        catch (Exception ex)
        {
            return Error("exception", ex.Message);
        }
    }

    static ItemId ParseItemId(string s)
    {
        if (string.IsNullOrEmpty(s)) return ItemId.None;
        
        var name = s.StartsWith("Item.") ? s.Substring(5) : s;
        
        foreach (ItemId val in Enum.GetValues(typeof(ItemId)))
        {
            if (val.ToString() == name)
                return val;
        }
        return ItemId.None;
    }

    static PartyMemberId? ParsePartyMemberId(string s)
    {
        if (string.IsNullOrEmpty(s)) return null;
        
        var name = s.StartsWith("PartyMember.") ? s.Substring(13) : s;
        
        foreach (PartyMemberId val in Enum.GetValues(typeof(PartyMemberId)))
        {
            if (val.ToString() == name)
                return val;
        }
        return null;
    }

    static ItemSlotId ParseItemSlotId(string s)
    {
        if (string.IsNullOrEmpty(s)) return ItemSlotId.RightHand;
        
        var name = s.ToLowerInvariant();
        
        return name switch
        {
            "righthand" => ItemSlotId.RightHand,
            "lefthand" => ItemSlotId.LeftHand,
            "chest" => ItemSlotId.Chest,
            "feet" => ItemSlotId.Feet,
            "head" => ItemSlotId.Head,
            "neck" => ItemSlotId.Neck,
            "tail" => ItemSlotId.Tail,
            "leftfinger" => ItemSlotId.LeftFinger,
            "rightfinger" => ItemSlotId.RightFinger,
            _ => ItemSlotId.RightHand
        };
    }

    string SaveGame(JsonNode? node)
    {
        var id = node?["id"]?.GetValue<int>() ?? 1;
        var name = node?["name"]?.GetValue<string>() ?? "AgentSave";
        return RaiseGameEvent($"save_game {id} {name}");
    }

    string LoadGame(JsonNode? node)
    {
        var id = node?["id"]?.GetValue<int>();
        if (id == null) return Error("missing_param", "Field 'id' is required");
        return RaiseGameEvent($"load_game {id}");
    }

    string Teleport(JsonNode? node)
    {
        var map = node?["map"]?.GetValue<string>();
        var x = node?["x"]?.GetValue<int>();
        var y = node?["y"]?.GetValue<int>();
        if (map == null || x == null || y == null)
            return Error("missing_param", "Fields 'map', 'x', 'y' are required");
        var dir = node?["direction"]?.GetValue<int>() ?? 0;
        return RaiseGameEvent($"teleport {map} {x} {y} {dir} 0 2");
    }

    string LoadMap(JsonNode? node)
    {
        var map = node?["map"]?.GetValue<string>();
        if (map == null) return Error("missing_param", "Field 'map' is required");
        return RaiseGameEvent($"load_map {map}");
    }

    string TalkNpc(JsonNode? node)
    {
        var npcId = node?["npc_id"]?.GetValue<string>();
        if (npcId == null) return Error("missing_param", "Field 'npc_id' is required");
        return RaiseGameEvent($"start_dialogue {npcId}");
    }

    string Respond(JsonNode? node)
    {
        var option = node?["option"]?.GetValue<int>();
        if (option == null) return Error("missing_param", "Field 'option' is required");
        return RaiseGameEvent($"respond {option}");
    }

    string ModifyGold(JsonNode? node)
    {
        var amount = node?["amount"]?.GetValue<int>();
        if (amount == null) return Error("missing_param", "Field 'amount' is required");
        return RaiseGameEvent($"modify_gold {amount}");
    }

    string RaiseGameEvent(string? eventString)
    {
        if (string.IsNullOrWhiteSpace(eventString))
            return Error("missing_param", "Field 'event' is required");

        var logExchange = TryResolve<ILogExchange>();
        if (logExchange == null)
            return Error("not_ready", "Log exchange not available");

        var parsed = Event.Parse(eventString, out var error);
        if (parsed == null)
            return Error("parse_error", $"Could not parse event '{eventString}': {error}");

        logExchange.EnqueueEvent(parsed);
        return Ok(new { raised = eventString });
    }

    // ── Broadcast ─────────────────────────────────────────────────────────────

    public void BroadcastEvent(string eventJson)
    {
        List<WebSocket> snapshot;
        lock (_eventLock) snapshot = [.. _eventClients];

        var bytes = Encoding.UTF8.GetBytes(eventJson);
        foreach (var ws in snapshot)
        {
            if (ws.State != WebSocketState.Open) continue;
            try { ws.SendAsync(bytes, WebSocketMessageType.Text, true, CancellationToken.None); }
            catch { }
        }
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    static string GetCharacterName(IEffectiveCharacterSheet? sheet, string fallback)
    {
        if (sheet == null) return fallback;
        foreach (var lang in new[] { Language.German, Language.English, Language.French })
        {
            try { return sheet.GetName(lang); }
            catch (InvalidOperationException) { }
        }
        return fallback;
    }

    static string Ok(object payload) =>
        JsonSerializer.Serialize(new { ok = true, result = payload });

    static string Error(string code, string message) =>
        JsonSerializer.Serialize(new { ok = false, error = code, message });

    // ── Monitor HTML ──────────────────────────────────────────────────────────

    const string MonitorHtml = """
        <!DOCTYPE html>
        <html lang="en">
        <head>
        <meta charset="UTF-8">
        <title>UAlbion Agent Monitor</title>
        <style>
          * { box-sizing: border-box; margin: 0; padding: 0; }
          body { background: #1a1a2e; color: #e0e0e0; font-family: Consolas, monospace; font-size: 13px; display: flex; flex-direction: column; height: 100vh; }
          header { background: #16213e; padding: 10px 16px; border-bottom: 1px solid #0f3460; display: flex; align-items: center; gap: 10px; }
          header h1 { color: #e94560; font-size: 15px; }
          #dot { width: 10px; height: 10px; border-radius: 50%; background: #555; }
          #dot.on { background: #4caf50; }
          #quickbar { background: #16213e; padding: 6px 12px; border-bottom: 1px solid #0f3460; display: flex; flex-wrap: wrap; gap: 4px; }
          .qbtn { background: #0f3460; color: #e0e0e0; border: 1px solid #2196f3; padding: 3px 10px; border-radius: 3px; cursor: pointer; font-size: 12px; font-family: inherit; }
          .qbtn:hover { background: #1a4a8a; }
          .qbtn.danger { border-color: #e94560; }
          #scriptpanel { background: #16213e; padding: 6px 12px; border-bottom: 1px solid #0f3460; }
          #log { flex: 1; overflow-y: auto; padding: 8px 12px; }
          .m { margin: 2px 0; padding: 4px 8px; border-radius: 3px; word-break: break-all; }
          .cmd  { background: #0d2137; border-left: 3px solid #2196f3; }
          .resp { background: #0d1f0d; border-left: 3px solid #4caf50; }
          .info { border-left: 3px solid #555; color: #777; }
          .err  { background: #1f0d0d; border-left: 3px solid #e94560; }
          .ts   { color: #555; font-size: 11px; margin-right: 6px; }
          .dir  { font-weight: bold; margin-right: 6px; }
          pre   { display: inline; white-space: pre-wrap; }
          details > summary { cursor: pointer; list-style: none; display: inline; }
          details > summary::before { content: '▶ '; font-size: 10px; color: #555; }
          details[open] > summary::before { content: '▼ '; }
          details > pre { margin-top: 4px; }
          footer { background: #16213e; padding: 8px 12px; border-top: 1px solid #0f3460; display: flex; gap: 8px; }
          #inp  { flex: 1; background: #0a0a1a; border: 1px solid #0f3460; color: #e0e0e0; padding: 6px 10px; border-radius: 4px; font-family: inherit; font-size: 13px; }
          #btn  { background: #e94560; color: #fff; border: none; padding: 6px 16px; border-radius: 4px; cursor: pointer; font-family: inherit; }
          #btn:hover { background: #c73652; }
        </style>
        </head>
        <body>
        <header>
          <div id="dot"></div>
          <h1>UAlbion Agent Monitor</h1>
        </header>
        <div id="quickbar"></div>
        <div id="scriptpanel">
          <details><summary style="cursor:pointer;color:#2196f3">▶ Python Scripts</summary>
            <div id="scriptbtns" style="display:flex;flex-wrap:wrap;gap:4px;margin-top:6px"></div>
          </details>
        </div>
        <div id="log"></div>
        <footer>
          <input id="inp" type="text" placeholder='{"cmd":"ping"}' spellcheck="false">
          <button id="btn">Send</button>
        </footer>
        <script>
        const logEl = document.getElementById('log');
        const dot = document.getElementById('dot');
        const inp = document.getElementById('inp');
        const btn = document.getElementById('btn');
        let agentWs = null;

        function ts() { return new Date().toTimeString().slice(0,8); }

        function esc(s) { return s.replace(/&/g,'&amp;').replace(/</g,'&lt;').replace(/>/g,'&gt;'); }

        function summarize(raw) {
          try {
            const m = JSON.parse(raw);
            if (!m) return raw;
            if (m.cmd === 'get_party' && m.party) {
              const members = m.party.map(p => `${p.name} HP ${p.hp}/${p.max_hp}`).join(', ');
              return `get_party: ${members}`;
            }
            if (m.cmd === 'get_combat' && m.tile_map) {
              const mobs = m.tile_map.filter(Boolean).length;
              return `get_combat: ${mobs} mob(s) on field, scene=${m.scene}`;
            }
            if (m.cmd === 'get_scene') return `get_scene: ${m.scene ?? '?'}`;
            if (m.type === 'command') return `→ ${m.cmd ?? raw.slice(0, 60)}`;
            if (m.type === 'response' && m.ok != null) return `← ok=${m.ok}${m.error ? ' err='+m.error : ''}`;
            if (m.sev) return `[${m.sev}] ${m.msg}`;
            return raw.slice(0, 80);
          } catch { return raw.slice(0, 80); }
        }

        function addRow(cls, dir, text) {
          let pretty = text;
          try { pretty = JSON.stringify(JSON.parse(text), null, 2); } catch {}
          const summary = summarize(text);
          const d = document.createElement('div');
          d.className = 'm ' + cls;
          d.innerHTML = `<span class="ts">${ts()}</span><span class="dir">${esc(dir)}</span>`
            + `<details><summary>${esc(summary)}</summary><pre>${esc(pretty)}</pre></details>`;
          logEl.appendChild(d);
          logEl.scrollTop = logEl.scrollHeight;
        }

        function sendRaw(json) {
          if (!agentWs || agentWs.readyState !== WebSocket.OPEN) {
            agentWs = new WebSocket('ws://' + location.host + '/agent');
            agentWs.onopen = () => agentWs.send(json);
            agentWs.onmessage = () => {};
            agentWs.onerror = () => addRow('err','!','Failed to connect');
          } else { agentWs.send(json); }
        }

        const QUICK_CMDS = [
          ['Ping',        '{"cmd":"ping"}'],
          ['New Game',    '{"cmd":"start_new_game"}'],
          ['Scene',       '{"cmd":"get_scene"}'],
          ['Party',       '{"cmd":"get_party"}'],
          ['Combat',      '{"cmd":"get_combat"}'],
          ['Equip Sword', '{"cmd":"equip_item","member":"PartyMember.Tom","item":"Item.Sword","slot":"RightHand"}'],
          ['2× Skrinn',   '{"cmd":"raise_event","event":"encounter MonsterGroup.TwoSkrinn1 CombatBackground.Dungeon"}'],
          ['3× Skrinn',   '{"cmd":"raise_event","event":"encounter MonsterGroup.ThreeSkrinn1 CombatBackground.Dungeon"}'],
          ['1× Krondir',  '{"cmd":"raise_event","event":"encounter MonsterGroup.OneKrondir1 CombatBackground.Dungeon"}'],
          ['Round',       '{"cmd":"raise_event","event":"begin_combat_round"}'],
          ['Dismiss',     '{"cmd":"raise_event","event":"dismiss_message"}'],
        ];

        function connectEvents() {
          const ws = new WebSocket('ws://' + location.host + '/events');
          ws.onopen  = () => { dot.className = 'on'; addRow('info','●','Connected to /events'); };
          ws.onclose = () => { dot.className = ''; addRow('info','●','Disconnected — reconnecting…'); setTimeout(connectEvents, 2000); };
          ws.onerror = () => {};
          ws.onmessage = e => {
            let cls = 'info', dir = '?';
            try {
              const m = JSON.parse(e.data);
              if (m.type === 'command')  { cls = 'cmd';  dir = '→ cmd'; }
              else if (m.type === 'response') { cls = 'resp'; dir = '← resp'; }
              else if (m.type === 'script_output') { cls = 'info'; dir = '🐍 ' + m.file; }
              else if (m.type === 'script_done') { cls = m.exit === 0 ? 'resp' : 'err'; dir = '🐍 done (' + m.exit + ')'; }
              else { dir = m.type || '?'; }
            } catch {}
            addRow(cls, dir, e.data);
          };
        }

        function sendCmd() {
          const text = inp.value.trim();
          if (!text) return;
          if (!agentWs || agentWs.readyState !== WebSocket.OPEN) {
            agentWs = new WebSocket('ws://' + location.host + '/agent');
            agentWs.onopen  = () => agentWs.send(text);
            agentWs.onmessage = () => {};
            agentWs.onerror = () => addRow('err','!','Failed to connect to /agent');
          } else {
            agentWs.send(text);
          }
          inp.value = '';
        }

        btn.onclick = sendCmd;
        inp.addEventListener('keydown', e => { if (e.key === 'Enter') sendCmd(); });

        const qb = document.getElementById('quickbar');
        for (const [label, json] of QUICK_CMDS) {
          const b = document.createElement('button');
          b.className = 'qbtn';
          b.textContent = label;
          b.onclick = () => sendRaw(json);
          qb.appendChild(b);
        }

        fetch('/scripts').then(r => r.json()).then(files => {
          const c = document.getElementById('scriptbtns');
          for (const f of files) {
            const b = document.createElement('button');
            b.className = 'qbtn';
            b.textContent = f.replace('.py','');
            b.onclick = () => fetch('/run-script?file=' + encodeURIComponent(f), {method:'POST'});
            c.appendChild(b);
          }
        });

        connectEvents();
        </script>
        </body>
        </html>
        """;
}
