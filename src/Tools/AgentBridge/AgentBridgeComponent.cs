using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
using UAlbion.Config;
using UAlbion.Formats;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Assets.Inv;
using UAlbion.Formats.Assets.Save;
using UAlbion.Formats.Assets.Sheets;
using UAlbion.Formats.Ids;
using UAlbion.Game.State;
using UAlbion.Game.Combat;
using UAlbion.Game.Events;
using UAlbion.Game.Events.Inventory;

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
    readonly CombatAgent _combatAgent = new();

    static readonly string ScriptDir = Path.Combine(
        AppContext.BaseDirectory, "..", "..", "..", "..", "Tools", "AgentBridge", "test");

    static string SequenceDir => ScriptDir;

    public void Dispose()
    {
        _cts.Dispose();
        _http.Close();
        GC.SuppressFinalize(this);
    }

    public AgentBridgeComponent()
    {
        AttachChild(_combatAgent);
        On<BeginFrameEvent>(_ => DrainCommandQueue());
        On<LogEvent>(BroadcastLog);
        On<EndCombatEvent>(e => BroadcastEvent(JsonSerializer.Serialize(
            new { type = "combat_ended", result = e.Result.ToString() })));
        On<InventoryChangedEvent>(OnInventoryChanged);
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
                else if (ctx.Request.Url?.AbsolutePath == "/sequences")
                {
                    switch (ctx.Request.HttpMethod)
                    {
                        case "GET":    _ = Task.Run(() => ServeSequenceListAsync(ctx), ct); break;
                        case "POST":   _ = Task.Run(() => SaveSequenceAsync(ctx), ct); break;
                        case "DELETE": _ = Task.Run(() => DeleteSequenceAsync(ctx), ct); break;
                        default: ctx.Response.StatusCode = 405; ctx.Response.Close(); break;
                    }
                }
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
                string? line;
                while ((line = await reader.ReadLineAsync()) != null)
                    BroadcastEvent(JsonSerializer.Serialize(new { type = "script_output", file, line }));
            }
            await Task.WhenAll(Relay(proc.StandardOutput), Relay(proc.StandardError));
            await proc.WaitForExitAsync();
            BroadcastEvent(JsonSerializer.Serialize(new { type = "script_done", file, exit = proc.ExitCode }));
        });
    }

    // ── Sequence REST handlers ─────────────────────────────────────────────────

    static async Task ServeSequenceListAsync(HttpListenerContext ctx)
    {
        // If ?file=name.seq.json → return full content of that sequence
        var fileParam = ctx.Request.QueryString["file"];
        if (!string.IsNullOrEmpty(fileParam))
        {
            var path = Path.Combine(SequenceDir, Path.GetFileName(fileParam));
            if (!File.Exists(path)) { ctx.Response.StatusCode = 404; ctx.Response.Close(); return; }
            var content = await File.ReadAllTextAsync(path);
            ctx.Response.ContentType = "application/json";
            ctx.Response.Headers["Access-Control-Allow-Origin"] = "*";
            var bytes = Encoding.UTF8.GetBytes(content);
            ctx.Response.ContentLength64 = bytes.Length;
            try { await ctx.Response.OutputStream.WriteAsync(bytes); ctx.Response.OutputStream.Close(); }
            catch { }
            return;
        }

        // Otherwise return metadata list
        var files = Directory.Exists(SequenceDir)
            ? Directory.GetFiles(SequenceDir, "*.seq.json")
                .Select(p =>
                {
                    try
                    {
                        var json = File.ReadAllText(p);
                        var node = JsonNode.Parse(json);
                        return (object)new
                        {
                            file = Path.GetFileName(p),
                            name = node?["name"]?.GetValue<string>() ?? Path.GetFileNameWithoutExtension(p),
                            description = node?["description"]?.GetValue<string>() ?? "",
                            steps = node?["steps"]?.AsArray().Count ?? 0
                        };
                    }
                    catch { return null; }
                })
                .Where(x => x != null)
                .ToArray()
            : Array.Empty<object>();

        await WriteJsonResponseAsync(ctx, files);
    }

    static async Task SaveSequenceAsync(HttpListenerContext ctx)
    {
        using var reader = new StreamReader(ctx.Request.InputStream);
        var body = await reader.ReadToEndAsync();

        JsonNode? node;
        try { node = JsonNode.Parse(body); }
        catch { ctx.Response.StatusCode = 400; ctx.Response.Close(); return; }

        var name = node?["name"]?.GetValue<string>();
        if (string.IsNullOrWhiteSpace(name)) { ctx.Response.StatusCode = 400; ctx.Response.Close(); return; }

        Directory.CreateDirectory(SequenceDir);
        var safeName = string.Concat(name.Select(c => Path.GetInvalidFileNameChars().Contains(c) ? '_' : c));
        var path = Path.Combine(SequenceDir, safeName + ".seq.json");
        await File.WriteAllTextAsync(path, body);

        await WriteJsonResponseAsync(ctx, new { saved = Path.GetFileName(path) });
    }

    static async Task DeleteSequenceAsync(HttpListenerContext ctx)
    {
        var file = ctx.Request.QueryString["file"];
        if (string.IsNullOrEmpty(file)) { ctx.Response.StatusCode = 400; ctx.Response.Close(); return; }

        var path = Path.Combine(SequenceDir, Path.GetFileName(file));
        if (File.Exists(path))
            File.Delete(path);

        await WriteJsonResponseAsync(ctx, new { deleted = file });
    }

    static async Task WriteJsonResponseAsync(HttpListenerContext ctx, object payload)
    {
        var json = JsonSerializer.Serialize(payload);
        ctx.Response.ContentType = "application/json";
        ctx.Response.Headers["Access-Control-Allow-Origin"] = "*";
        var bytes = Encoding.UTF8.GetBytes(json);
        ctx.Response.ContentLength64 = bytes.Length;
        try { await ctx.Response.OutputStream.WriteAsync(bytes); ctx.Response.OutputStream.Close(); }
        catch { }
    }

    // ── WebSocket ─────────────────────────────────────────────────────────────

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
            "get_items"            => GetItems(),
            "get_encounters"       => GetEncounters(),
            "equip_item"             => EquipItem(node),
            "save_game"              => SaveGame(node),
            "load_game"              => LoadGame(node),
            "teleport"               => Teleport(node),
            "load_map"               => LoadMap(node),
            "talk_npc"        => TalkNpc(node),
            "respond"         => Respond(node),
            "modify_gold"     => ModifyGold(node),
            "modify_hp"        => ModifyHp(node),
            "modify_status"    => ModifyStatus(node),
            "raise_event"        => RaiseGameEvent(node?["event"]?.GetValue<string>()),
            "send_input_action"  => SendInputAction(node),
            "start_new_game"     => StartNewGame(node),
            "dismiss_message"    => RaiseGameEvent("dismiss_message"),
            "enter_merchant"     => EnterMerchant(node),
            "toggle_clock"       => ToggleSpecialItem(Base.Item.Clock, ActiveItems.Clock),
            "toggle_compass"     => ToggleSpecialItem(Base.Item.Compass, ActiveItems.Compass),
            "toggle_monster_eye" => ToggleSpecialItem(Base.Item.MonsterEye, ActiveItems.MonsterEye),
            "get_active_items"   => GetActiveItems(),
            "quicksave"          => RaiseGameEvent("quicksave"),
            "quickload"          => RaiseGameEvent("quickload"),
            "auto_combat_start"  => AutoCombatStart(node),
            "auto_combat_stop"   => AutoCombatStop(),
            "auto_combat_status" => AutoCombatStatus(),
            _ => Error("unknown_command",
                $"Unknown command '{cmd}'. Available: ping, quit, get_scene, get_party, get_inventory, get_map, get_time, get_npcs, " +
                "get_combat, select_combat_action, select_combat_target, equip_item, save_game, load_game, teleport, load_map, " +
                "talk_npc, respond, modify_gold, modify_hp, modify_status, raise_event, send_input_action, start_new_game, dismiss_message, enter_merchant, " +
                "toggle_clock, toggle_compass, toggle_monster_eye, get_active_items")
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

        var planningState = battle == null ? "NotInCombat" : battle.PlanningState.ToString();
        var pendingActor  = battle?.PendingActorPosition;

        return Ok(new { scene = sceneId, planning_state = planningState, pending_actor = pendingActor, party = partyInCombat, tile_map = tileMap });
    }

    string SelectCombatAction(JsonNode? node)
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

    string SelectCombatTarget(JsonNode? node)
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

    static readonly PlayerConditions[] ConditionFlags =
    {
        PlayerConditions.Unconscious, PlayerConditions.Poisoned, PlayerConditions.Ill,
        PlayerConditions.Exhausted, PlayerConditions.Paralysed, PlayerConditions.Fleeing,
        PlayerConditions.Intoxicated, PlayerConditions.Blind, PlayerConditions.Panicking,
        PlayerConditions.Asleep, PlayerConditions.Insane, PlayerConditions.Irritated
    };

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
            var effective = player.Effective;
            var conditions = effective?.Combat.Conditions ?? 0;
            var conditionsList = new List<string>();
            foreach (var flag in ConditionFlags)
                if ((conditions & flag) != 0)
                    conditionsList.Add(flag.ToString());

            members.Add(new
            {
                id = player.Id.ToString(),
                name = GetCharacterName(sheet, player.Id.ToString()),
                hp = sheet?.Combat.LifePoints.Current ?? 0,
                max_hp = sheet?.Combat.LifePoints.Max ?? 0,
                level = sheet?.Level ?? 0,
                class_name = sheet?.PlayerClass.ToString() ?? "",
                experience_points = sheet?.Combat.ExperiencePoints ?? 0,
                combat_position = player.CombatPosition,
                conditions = conditionsList.ToArray()
            });
        }

        return Ok(new { leader = party.Leader?.Id.ToString(), gold = party.TotalGold, members });
    }

// ── Action commands ───────────────────────────────────────────────────────

    static string GetEncounters()
    {
        var groups = new List<object>();
        foreach (Base.MonsterGroup mg in Enum.GetValues<Base.MonsterGroup>())
        {
            if (mg == Base.MonsterGroup.Empty || mg == Base.MonsterGroup.DebugMix) continue;
            groups.Add(new { id = "MonsterGroup." + mg.ToString(), name = mg.ToString() });
        }

        return Ok(new { encounters = groups });
    }

    string GetItems()
    {
        var state = TryResolve<IGameState>();
        if (state == null || !state.Loaded)
            return Error("not_ready", "No game loaded.");

        var assets = TryResolve<IAssetManager>();
        if (assets == null)
            return Error("not_ready", "Asset manager not available.");

        var typeDisplayNames = new Dictionary<ItemType, string>
        {
            { ItemType.CloseRangeWeapon, "Melee Weapons" },
            { ItemType.LongRangeWeapon, "Ranged Weapons" },
            { ItemType.Ammo, "Ammo" },
            { ItemType.Armor, "Body Armor" },
            { ItemType.Helmet, "Helmets" },
            { ItemType.Shoes, "Shoes" },
            { ItemType.Shield, "Shields" },
            { ItemType.Amulet, "Amulets" },
            { ItemType.MagicRing, "Rings" },
            { ItemType.SpellScroll, "Spell Scrolls" },
            { ItemType.Drink, "Drinks" },
            { ItemType.MagicItem, "Magic Items" },
            { ItemType.Valuable, "Valuables" },
            { ItemType.Tool, "Tools" },
            { ItemType.Key, "Keys" },
            { ItemType.LightSource, "Light Sources" },
            { ItemType.Lockpick, "Lockpicks" },
            { ItemType.Document, "Documents" },
            { ItemType.HeadsUpDisplayItem, "HUD Items" },
            { ItemType.Misc, "Misc" },
        };

        var allItemIds = AssetMapping.Global.EnumerateAssetsOfType(AssetType.Item);
        var itemsByType = new List<object>();

        foreach (var typeIdGroup in allItemIds
            .Select(id => (ItemId)id)
            .Select(id => assets.LoadItem(id))
            .Where(d => d != null)
            .GroupBy(d => d.TypeId)
            .OrderBy(g => g.Key))
        {
            var typeName = typeDisplayNames.TryGetValue(typeIdGroup.Key, out var dn) ? dn : typeIdGroup.Key.ToString();
            var items = typeIdGroup.OrderBy(d => d.Id).Select(d =>
            {
                string name;
                try { name = assets.LoadStringSafe(d.Name); }
                catch { name = d.Id.ToString(); }
                return new
                {
                    id = d.Id.ToString(),
                    name,
                    damage = d.Damage,
                    protection = d.Protection,
                    weight = d.Weight,
                    value = d.Value,
                    charges = d.MaxCharges > 0 ? $"{d.Charges}/{d.MaxCharges}" : null,
                    spell = d.Spell.ToString(),
                };
            }).ToArray();

            itemsByType.Add(new { type = typeIdGroup.Key.ToString(), label = typeName, items });
        }

        return Ok(new { categories = itemsByType });
    }

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

        var partyMemberId = ParsePartyMemberId(member);
        if (partyMemberId == null)
            return Error("invalid_member", $"Unknown party member '{member}'");

        var itemId = ParseItemId(item);
        if (itemId.IsNone)
            return Error("invalid_item", $"Unknown item '{item}'");

        var slotId = ParseItemSlotId(slot);

        IPlayer? player = null;
        foreach (var p in state.Party.WalkOrder)
        {
            if (p.Id == partyMemberId)
            {
                player = p;
                break;
            }
        }

        if (player == null)
            return Error("not_in_party", $"Party member '{member}' is not in the current party");

        var invId = new InventoryId(player.Id);
        var inv = ((GameState)state).GetWriteableInventory(invId);
        if (inv == null)
            return Error("not_found", $"Inventory not found for '{member}'");

        if (inv is not Inventory writableInv)
            return Error("not_supported", $"Inventory type {inv.GetType()} is not writable");

        var itemSlot = writableInv.Slots[(int)slotId];
        itemSlot.Item = itemId;
        itemSlot.Amount = 1;

        Raise(new InventoryChangedEvent(invId));

        return Ok(new { equipped = item, member = member, slot = slot });
    }

    static ItemId ParseItemId(string s)
    {
        if (string.IsNullOrEmpty(s)) return ItemId.None;

        var name = s.StartsWith("Item.", StringComparison.Ordinal) ? s.Substring(5) : s;

        try { return ItemId.Parse("Item." + name); }
        catch { return ItemId.None; }
    }

    static PartyMemberId? ParsePartyMemberId(string s)
    {
        if (string.IsNullOrEmpty(s)) return null;

        var name = s.StartsWith("PartyMember.", StringComparison.Ordinal) ? s.Substring(12) : s;

        try { return PartyMemberId.Parse("PartyMember." + name); }
        catch { return null; }
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

    string EnterMerchant(JsonNode? node)
    {
        var merchantId = node?["merchant_id"]?.GetValue<string>();
        if (merchantId == null)
            return Error("missing_param", "'merchant_id' required (e.g. \"Merchant.Wania\", \"Merchant.100\")");
        var memberId = node?["member"]?.GetValue<string>() ?? "PartyMember.None";
        return RaiseGameEvent($"inv:merchant {merchantId} {memberId}");
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
        // Gold is stored at 10× scale (display = stored / 10), so multiply input by 10.
        int raw = amount.Value * 10;
        if (raw >= 0)
            return RaiseGameEvent($"modify_gold AddAmount {raw}");
        return RaiseGameEvent($"modify_gold SubtractAmount {-raw}");
    }

    string ModifyHp(JsonNode? node)
    {
        var member = node?["member"]?.GetValue<string>();
        var amount = node?["amount"]?.GetValue<int>();
        var mode = node?["mode"]?.GetValue<string>() ?? "add";

        if (member == null) return Error("missing_param", "Field 'member' is required (e.g. 'PartyMember.Tom')");

        var state = TryResolve<IGameState>();
        if (state == null || !state.Loaded)
            return Error("not_ready", "No game loaded.");

        // Find the player by matching id or name
        IPlayer? player = null;
        var memberId = ParsePartyMemberId(member);
        foreach (var p in state.Party.WalkOrder)
        {
            if (p.Id == memberId) { player = p; break; }
            // Fallback: match by id string (e.g. "PartyMember.Tom")
            if (member.Equals(p.Id.ToString(), StringComparison.Ordinal)) { player = p; break; }
            // Fallback: match by name (e.g. "Tom")
            var effective = p.Effective;
            if (effective != null)
            {
                try
                {
                    var ename = effective.GetName(Base.Language.English);
                    var dname = effective.GetName(Base.Language.German);
                    if (member.Equals(ename, StringComparison.OrdinalIgnoreCase) ||
                        member.Equals(dname, StringComparison.OrdinalIgnoreCase))
                    {
                        player = p;
                        break;
                    }
                }
                catch { }
            }
        }
        if (player == null)
            return Error("not_in_party", $"Party member '{member}' is not in the current party");

        var sheet = ((GameState)state).GetWriteableSheet(player.Id);
        if (sheet == null)
            return Error("not_found", $"Sheet not found for '{member}'");

        var lp = sheet.Combat.LifePoints;
        int delta;
        if (mode == "full")
        {
            delta = lp.Max - lp.Current;
            lp.Current = (ushort)lp.Max;
        }
        else
        {
            delta = amount ?? 0;
            int newHp = Math.Clamp(lp.Current + delta, 0, lp.Max);
            lp.Current = (ushort)newHp;
        }

        Raise(new InventoryChangedEvent(new InventoryId(sheet.Id)));
        Raise(new SheetChangedEvent(sheet.Id));

        return Ok(new { member = player.Id.ToString(), hp = lp.Current, max_hp = lp.Max, delta = delta });
    }

    string ModifyStatus(JsonNode? node)
    {
        var member = node?["member"]?.GetValue<string>();
        var condition = node?["condition"]?.GetValue<string>();
        var operation = node?["operation"]?.GetValue<string>() ?? "toggle";

        if (member == null) return Error("missing_param", "Field 'member' is required");
        if (condition == null) return Error("missing_param", "Field 'condition' is required (e.g. 'Unconscious', 'Poisoned', 'Ill')");

        var state = TryResolve<IGameState>();
        if (state == null || !state.Loaded)
            return Error("not_ready", "No game loaded.");

        IPlayer? player = null;
        var memberId = ParsePartyMemberId(member);
        foreach (var p in state.Party.WalkOrder)
        {
            if (p.Id == memberId) { player = p; break; }
            if (member.Equals(p.Id.ToString(), StringComparison.Ordinal)) { player = p; break; }
            var effective = p.Effective;
            if (effective != null)
            {
                try
                {
                    var ename = effective.GetName(Base.Language.English);
                    var dname = effective.GetName(Base.Language.German);
                    if (member.Equals(ename, StringComparison.OrdinalIgnoreCase) ||
                        member.Equals(dname, StringComparison.OrdinalIgnoreCase))
                    {
                        player = p;
                        break;
                    }
                }
                catch { }
            }
        }
        if (player == null)
            return Error("not_in_party", $"Party member '{member}' is not in the current party");

        var sheet = ((GameState)state).GetWriteableSheet(player.Id);
        if (sheet == null)
            return Error("not_found", $"Sheet not found for '{member}'");

        PlayerCondition pc;
        try { pc = Enum.Parse<PlayerCondition>(condition, true); }
        catch { return Error("invalid_condition", $"Unknown condition '{condition}'. Valid: Unconscious, Poisoned, Ill, Exhausted, Paralysed, Fleeing, Intoxicated, Blind, Panicking, Asleep, Insane, Irritated"); }

        var flag = pc.ToFlag();
        var existing = sheet.Combat.Conditions;

        switch (operation.ToLowerInvariant())
        {
            case "toggle":
                sheet.Combat.Conditions = existing ^ flag;
                break;
            case "set":
            case "on":
            case "enable":
                sheet.Combat.Conditions = existing | flag;
                break;
            case "clear":
            case "off":
            case "disable":
                sheet.Combat.Conditions = existing & ~flag;
                break;
            default:
                return Error("invalid_operation", $"Operation must be 'toggle', 'set', or 'clear'");
        }

        Raise(new SheetChangedEvent(sheet.Id));

        var nowHas = (sheet.Combat.Conditions & flag) != 0;
        return Ok(new { member = player.Id.ToString(), condition = condition, active = nowHas, operation = operation });
    }

    string SendInputAction(JsonNode? node)
    {
        var action = node?["action"]?.GetValue<string>();
        if (string.IsNullOrWhiteSpace(action))
            return Error("missing_param", "Field 'action' is required (action string from input.json)");

        if (action.StartsWith('!'))
            return Error("not_supported", $"Internal engine action '{action}' cannot be dispatched via AgentBridge");

        // Strip '+' prefix: continuous/held actions fire once when sent from the companion
        var eventString = action.TrimStart('+').Trim();
        return RaiseGameEvent(eventString);
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

    string StartNewGame(JsonNode? node)
    {
        var mapName = node?["map"]?.GetValue<string>() ?? "TorontoBegin";
        var x = (ushort)(node?["x"]?.GetValue<ushort>() ?? 31);
        var y = (ushort)(node?["y"]?.GetValue<ushort>() ?? 76);

        var mapId = mapName.Contains('.') ? MapId.Parse(mapName) : (MapId)AssetMapping.Global.Parse(mapName, MapId.ValidTypes);

        var logExchange = TryResolve<ILogExchange>();
        if (logExchange == null)
            return Error("not_ready", "Log exchange not available");

        // Bypass MainMenu Yes/No prompt — directly enqueue NewGameEvent
        logExchange.EnqueueEvent(new NewGameEvent(mapId, x, y));
        return Ok(new { raised = $"new_game {mapName} {x} {y}" });
    }

    // ── Special item (HDOB overlay) toggles ───────────────────────────────────

    string ToggleSpecialItem(ItemId itemId, ActiveItems flag)
    {
        var state = TryResolve<IGameState>();
        if (state == null || !state.Loaded)
            return Error("not_ready", "No game loaded.");

        var currentlyActive = (state.ActiveItems & flag) != 0;
        var newState = !currentlyActive;
        Raise(new SetSpecialItemActiveEvent(itemId, newState));
        return Ok(new { item = itemId.ToString(), active = newState });
    }

    string GetActiveItems()
    {
        var state = TryResolve<IGameState>();
        if (state == null || !state.Loaded)
            return Error("not_ready", "No game loaded.");

        var items = state.ActiveItems;
        return Ok(new
        {
            compass = (items & ActiveItems.Compass) != 0,
            monster_eye = (items & ActiveItems.MonsterEye) != 0,
            clock = (items & ActiveItems.Clock) != 0,
            raw = (uint)items
        });
    }

    // ── Auto-combat commands ──────────────────────────────────────────────────

    string AutoCombatStart(JsonNode? node)
    {
        _combatAgent.Strategy.Enabled         = true;
        _combatAgent.Strategy.AttackPriority  = node?["attack_priority"]?.GetValue<string>() ?? _combatAgent.Strategy.AttackPriority;
        _combatAgent.Strategy.HealThreshold   = node?["heal_threshold"]?.GetValue<float>()   ?? _combatAgent.Strategy.HealThreshold;
        BroadcastEvent(JsonSerializer.Serialize(new { type = "auto_combat", enabled = true, strategy = new
        {
            attack_priority = _combatAgent.Strategy.AttackPriority,
            heal_threshold  = _combatAgent.Strategy.HealThreshold
        }}));
        return Ok(new { auto_combat = true, attack_priority = _combatAgent.Strategy.AttackPriority, heal_threshold = _combatAgent.Strategy.HealThreshold });
    }

    string AutoCombatStop()
    {
        _combatAgent.Strategy.Enabled = false;
        BroadcastEvent(JsonSerializer.Serialize(new { type = "auto_combat", enabled = false }));
        return Ok(new { auto_combat = false });
    }

    string AutoCombatStatus() =>
        Ok(new
        {
            enabled         = _combatAgent.Strategy.Enabled,
            attack_priority = _combatAgent.Strategy.AttackPriority,
            heal_threshold  = _combatAgent.Strategy.HealThreshold
        });

    // ── Broadcast ─────────────────────────────────────────────────────────────

    void OnInventoryChanged(InventoryChangedEvent e)
    {
        if (!e.IsRealChange) return;
        if (e.Id.Type != InventoryType.Player) return;

        var state = TryResolve<IGameState>();
        if (state == null) return;

        var assets = TryResolve<IAssetManager>();
        if (assets == null) return;

        var partyMemberId = new PartyMemberId(e.Id.Id);
        var member = state.Party[partyMemberId];
        if (member == null) return;

        // Read from writable inventory (ground truth), not the interpolated apparent copy
        var inv = ((GameState)state).GetWriteableInventory(e.Id);
        if (inv == null) return;

        var bodyParts = new Dictionary<string, object>();
        foreach (var slotId in new[] { ItemSlotId.Head, ItemSlotId.Neck, ItemSlotId.Chest, ItemSlotId.Feet,
                                       ItemSlotId.RightHand, ItemSlotId.LeftHand, ItemSlotId.RightFinger, ItemSlotId.LeftFinger,
                                       ItemSlotId.Tail })
        {
            var slot = inv.GetSlot(slotId);
            if (slot != null && slot.Item.Type == AssetType.Item)
            {
                var itemData = assets.LoadItem(slot.Item);
                var itemName = itemData != null ? assets.LoadStringSafe(itemData.Name) : slot.Item.ToString();
                bodyParts[slotId.ToString()] = new { item = itemName, charges = slot.Charges, broken = (slot.Flags & ItemSlotFlags.Broken) != 0, cursed = (slot.Flags & ItemSlotFlags.Cursed) != 0 };
            }
            else
            {
                bodyParts[slotId.ToString()] = null;
            }
        }

        BroadcastEvent(JsonSerializer.Serialize(new
        {
            type = "inventory_changed",
            member = member.Id.ToString(),
            equip = bodyParts
        }));
    }

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
          #hotkeyspanel { background: #16213e; padding: 6px 12px; border-bottom: 1px solid #0f3460; }
          .htabs { display: flex; gap: 2px; margin-bottom: 4px; flex-wrap: wrap; }
          .htab { background: #0a0a1a; border: 1px solid #334; color: #888; padding: 2px 10px; border-radius: 3px 3px 0 0; cursor: pointer; font-size: 11px; font-family: inherit; }
          .htab.active { background: #0f3460; color: #e0e0e0; border-color: #2196f3; }
          .hpane { display: none; flex-wrap: wrap; gap: 3px; padding: 4px 0; }
          .hpane.active { display: flex; }
          .hpane.scrollable { overflow-y: auto; max-height: 260px; align-content: flex-start; }
          .hbtn { background: #0a1a2a; color: #90c4e8; border: 1px solid #1565c0; padding: 2px 9px; border-radius: 3px; cursor: pointer; font-size: 11px; font-family: inherit; }
          .hbtn:hover { background: #1a3a5a; }
          .hbtn[data-cont] { border-color: #388e3c; color: #90e898; }
          .item-cat-btn { background: #0a0a1a; color: #aaa; border: 1px solid #444; padding: 2px 8px; border-radius: 3px; cursor: pointer; font-size: 10px; font-family: inherit; margin: 1px; }
          .item-cat-btn.active { background: #1a3a5a; color: #e0e0e0; border-color: #2196f3; }
          .item-row { background: #0a1220; border: 1px solid #1a2a3a; padding: 2px 6px; border-radius: 2px; cursor: pointer; font-size: 11px; display: inline-block; margin: 1px; }
          .item-row:hover { background: #1a3a5a; }
          #log { flex: 1; overflow-y: auto; padding: 8px 12px; }
          .m { margin: 2px 0; padding: 4px 8px; border-radius: 3px; word-break: break-all; }
          .cmd  { background: #0d2137; border-left: 3px solid #2196f3; }
          .resp { background: #0d1f0d; border-left: 3px solid #4caf50; }
          .resp.err-resp { background: #1f0d0d; border-left-color: #e94560; }
          .info { border-left: 3px solid #555; color: #777; }
          .err  { background: #1f0d0d; border-left: 3px solid #e94560; }
          .ts   { color: #555; font-size: 11px; margin-right: 6px; }
          .dir  { font-weight: bold; margin-right: 6px; }
          .status-icon { font-weight: bold; margin-right: 4px; font-size: 14px; }
          .status-icon.ok { color: #4caf50; }
          .status-icon.err { color: #e94560; }
          pre   { display: inline; white-space: pre-wrap; }
          details > summary { cursor: pointer; list-style: none; display: inline; }
          details > summary::before { content: '▶ '; font-size: 10px; color: #555; }
          details[open] > summary::before { content: '▼ '; }
          details > pre { margin-top: 4px; }
          .equip-panel { width: 100%; }
          .equip-char-select { display: flex; gap: 3px; margin-bottom: 6px; flex-wrap: wrap; }
          .equip-grid { display: flex; flex-wrap: wrap; gap: 3px; }
          .encounter-scroll { overflow-y: auto; max-height: 300px; }
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
        <div id="hotkeyspanel">
          <div class="htabs" id="htabs"></div>
          <div id="hpanes" class="scrollable"></div>
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
            if (m.cmd === 'get_active_items' && m.clock !== undefined) {
              const items = [];
              if (m.compass) items.push('Compass');
              if (m.monster_eye) items.push('MonsterEye');
              if (m.clock) items.push('Clock');
              return `active items: ${items.length ? items.join(', ') : '(none)'}`;
            }
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

          let statusHtml = '';
          if (cls === 'resp') {
            try {
              const parsed = JSON.parse(text);
              const payload = parsed.payload ? JSON.parse(parsed.payload) : parsed;
              const isOk = payload.ok !== false && parsed.ok !== false;
              if (isOk) {
                statusHtml = '<span class="status-icon ok">✓</span>';
              } else {
                statusHtml = '<span class="status-icon err">✗</span>';
                cls = 'resp err-resp';
              }
            } catch {}
          } else if (cls === 'err') {
            statusHtml = '<span class="status-icon err">✗</span>';
          }

          d.className = 'm ' + cls;
          d.innerHTML = `<span class="ts">${ts()}</span><span class="dir">${esc(dir)}</span>${statusHtml}`
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
          ['Quicksave',   '{"cmd":"quicksave"}'],
          ['Quickload',   '{"cmd":"quickload"}'],
          ['Scene',       '{"cmd":"get_scene"}'],
          ['Party',       '{"cmd":"get_party"}'],
          ['Combat',      '{"cmd":"get_combat"}'],
          ['Round',       '{"cmd":"raise_event","event":"begin_combat_round"}'],
          ['Dismiss',     '{"cmd":"raise_event","event":"dismiss_message"}'],
        ];

        function connectEvents() {
          const ws = new WebSocket('ws://' + location.host + '/events');
          ws.onopen  = () => { dot.className = 'on'; addRow('info','●','Connected to /events'); };
          ws.onclose = () => {
            dot.className = '';
            addRow('info','●','Disconnected — reconnecting…');
            for (const w of pendingEventWaiters) { clearTimeout(w.timer); w.reject(new Error('disconnected')); }
            pendingEventWaiters = [];
            setTimeout(connectEvents, 2000);
          };
          ws.onerror = () => {};
          ws.onmessage = e => {
            try {
              const outer = JSON.parse(e.data);
              // Notify any promise-based event waiters
              pendingEventWaiters = pendingEventWaiters.filter(w => {
                if (outer.type === w.type) { clearTimeout(w.timer); w.resolve(outer); return false; }
                return true;
              });
              if (outer.type === 'response' && outer.payload) {
                const inner = JSON.parse(outer.payload);
                // Auto-fetch party after start_new_game so tabs populate automatically
                if (inner.cmd === 'start_new_game' && inner.ok !== false) {
                  setTimeout(() => fetchPartyWithRetry(), 600);
                }
                if (inner.result && inner.result.members) {
                  buildCharactersPane(inner.result.members, inner.result.gold);
                  if (merchantTabBuilt) buildMerchantList();
                  if (equipTabBuilt && itemCategories.length) buildItemPicker(itemCategories);
                  if (partyPollTimer) { clearTimeout(partyPollTimer); partyPollTimer = null; }
                }
                if (inner.result && inner.result.categories) {
                  buildItemPicker(inner.result.categories);
                }
                if (inner.result && inner.result.encounters) {
                  buildEncounterList(inner.result.encounters);
                }
              }
            } catch {}
            let cls = 'info', dir = '?';
            try {
              const m = JSON.parse(e.data);
              if (m.type === 'command')  { cls = 'cmd';  dir = '→ cmd'; }
              else if (m.type === 'response') {
                const payload = m.payload ? JSON.parse(m.payload) : m;
                cls = (payload.ok === false) ? 'err' : 'resp';
                dir = '← resp';
              }
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

        const HOTKEYS = {
          Global: [
            ['dismiss_message','Dismiss'],['focus_console','Console'],
            ['e:toggle_fullscreen','Fullscreen'],['mute','Mute'],
            ['engine_flag toggle RenderDepth','Depth'],
            ['engine_flag toggle ShowBoundingBoxes','Bounds'],
            ['engine_flag toggle VSync','VSync'],
            ['push_scene Editor','Editor'],['push_mouse_mode DebugPick','DebugPick'],
            ['hide_debug_window','Hide'],['e:run_renderdoc','RenderDoc'],
            ['toggle_diagnostics','Diag'],['toggle_clock','Clock'],
            ['debug_run','Run'],['debug_break','Break'],['debug_step','Step'],
            ['update 1','+1'],['load_map_prompt','Load Map'],
            ['set_language English','EN'],['set_language German','DE'],['set_language French','FR'],
            ['quit','Quit'],
          ],
          World2D: [
            ['+party_move 0 -1','↑',true],['+party_move 0 1','↓',true],
            ['+party_move -1 0','←',true],['+party_move 1 0','→',true],
            ['noclip','Noclip'],['toggle_underlay','Underlay'],['toggle_overlay','Overlay'],
            ['e:mag -1','Zoom−'],['e:mag 1','Zoom+'],
            ['debug_flag toggle FastMovement','FastMove'],
            ['debug_flag toggle CollisionLayer','Collision'],['debug_flag toggle SitLayer','Sit'],
            ['debug_flag toggle ShowDebugTiles','Tiles'],['debug_flag toggle ZoneLayer','Zone'],
            ['debug_flag toggle NpcColliderLayer','NpcCol'],['debug_flag toggle NpcPathLayer','NpcPath'],
            ['inv:open Tom','Inv Tom'],
            ['+cursor_mode examine','Examine',true],['+cursor_mode manipulate','Manip',true],
            ['+cursor_mode talk','Talk',true],['+cursor_mode take','Take',true],
            ['push_scene MainMenu','MainMenu'],
          ],
          World3D: [
            ['+camera_move 0 -4','Cam↑',true],['+camera_move 0 4','Cam↓',true],
            ['+camera_move -4 0','Cam←',true],['+camera_move 4 0','Cam→',true],
            ['+party_move 0 -1','↑',true],['+party_move 0 1','↓',true],
            ['+party_move -1 0','←',true],['+party_move 1 0','→',true],
            ['+cam_rotate 0 0.03','Pitch↑',true],['+cam_rotate 0 -0.03','Pitch↓',true],
            ['+cam_rotate 0.03 0','Yaw←',true],['+cam_rotate -0.03 0','Yaw→',true],
            ['open_map','Map'],['wait','Wait'],['toggle_mlook','MLook'],
            ['set_mouse_mode MouseLook','MLook On'],['set_mouse_mode Normal','MLook Off'],
            ['inv:open Tom','Inv Tom'],
            ['+cursor_mode examine','Examine',true],['+cursor_mode manipulate','Manip',true],
            ['+cursor_mode talk','Talk',true],['+cursor_mode take','Take',true],
            ['push_scene MainMenu','MainMenu'],
          ],
          Inventory: [
            ['take_all','Take All'],['inv:equip','Equip'],['inv:close','Close'],
            ['inv:set_page Summary','Summary'],['inv:set_page Stats','Stats'],['inv:set_page Misc','Misc'],
          ],
          Conversation: [
            ['respond 1','1'],['respond 2','2'],['respond 3','3'],['respond 4','4'],['respond 5','5'],
            ['respond 6','6'],['respond 7','7'],['respond 8','8'],['respond 9','9'],
            ['enter_word','Enter Word'],
          ],
          MainMenu: [
            ['new_game TorontoBegin 31 76','New Game'],
            ['new_game Jirinaar 20 20','@ Jirinaar'],
            ['close_window','Close'],
          ],
          Core: [
            ['engine_flag toggle SuppressLayout','SuppressLayout'],
            ['debug_flag toggle NpcColliderLayer','NpcCollider'],['debug_flag toggle NpcPathLayer','NpcPath'],
          ],
          Overlays: [
            ['get_active_items','⟳ Refresh'],['toggle_clock','Clock'],['toggle_compass','Compass'],['toggle_monster_eye','MonsterEye'],
          ],
          Characters: [],
          Equip: [],
          Combat: [],
          Merchant: [],
          Sequences: [],
          AutoCombat: [],
        };

        const STATUS_CONDITIONS = [
          'Unconscious','Poisoned','Ill','Exhausted','Paralysed',
          'Fleeing','Intoxicated','Blind','Panicking','Asleep','Insane','Irritated'
        ];

        let partyMembers = [];
        let charsTabBuilt = false;
        let itemCategories = [];
        let equipTabBuilt = false;
        let selectedEquipChar = null;
        let combatTabBuilt = false;
        let merchantTabBuilt = false;
        let selectedMerchantChar = null;
        let sequencesTabBuilt = false;
        let autoCombatTabBuilt = false;
        let seqRunning = false;
        let seqAbort = false;
        let editingSequence = null;
        let seqResults = [];
        let allEncounters = [];
        let csFights = [];
        let csEquipItems = [];
        let pendingEventWaiters = [];

        function fetchPartyAndBuild() { sendRaw(JSON.stringify({cmd:'get_party'})); }
        let partyPollTimer = null;
        function fetchPartyWithRetry(maxAttempts) {
          if (partyPollTimer) { clearTimeout(partyPollTimer); partyPollTimer = null; }
          let attempts = 0;
          const max = maxAttempts || 8;
          function attempt() {
            sendRaw(JSON.stringify({cmd:'get_party'}));
            attempts++;
            if (attempts < max && !partyMembers.length) {
              partyPollTimer = setTimeout(attempt, 500);
            }
          }
          attempt();
        }
        function fetchItems() { sendRaw(JSON.stringify({cmd:'get_items'})); }
        function fetchEncounters() { sendRaw(JSON.stringify({cmd:'get_encounters'})); }

        function buildCharactersPane(members, partyGold) {
          partyMembers = members || [];
          const panesEl = document.getElementById('hpanes');
          let pane = panesEl.querySelector('[data-pane="Characters"]');
          if (!pane) {
            pane = document.createElement('div');
            pane.className = 'hpane';
            pane.dataset.pane = 'Characters';
            panesEl.appendChild(pane);
          }
          pane.innerHTML = '';

          if (partyMembers.length === 0) {
            pane.innerHTML = '<span style="color:#888;padding:4px;">No party data.</span>';
            return;
          }

          const refreshBtn = document.createElement('button');
          refreshBtn.className = 'hbtn';
          refreshBtn.textContent = '⟳ Refresh';
          refreshBtn.style.cssText = 'border-color:#ff9800;color:#e8c890;width:100%;margin-bottom:4px;';
          refreshBtn.onclick = fetchPartyAndBuild;
          pane.appendChild(refreshBtn);

          // Gold section
          const goldRow = document.createElement('div');
          goldRow.style.cssText = 'display:flex;align-items:center;gap:6px;margin-bottom:8px;background:#0a1220;border:1px solid #997700;border-radius:4px;padding:4px 10px;';
          goldRow.innerHTML = `
            <span style="color:#ffd080;font-size:13px;">💰</span>
            <span style="color:#aaa;font-size:11px;">Party Gold:</span>
            <span style="color:#ffd080;font-size:13px;font-weight:bold;flex:1;">${partyGold !== undefined ? partyGold : '—'}</span>
            <input id="gold-amount" type="number" value="100" min="1" style="width:70px;background:#0a0a1a;border:1px solid #334;color:#e0e0e0;padding:2px 5px;border-radius:3px;font-size:11px;font-family:inherit;">
            <button class="hbtn" style="border-color:#ffd080;color:#ffd080;font-size:10px;" onclick="addGold(1)">+ Give</button>
            <button class="hbtn" style="border-color:#888;color:#888;font-size:10px;" onclick="addGold(-1)">− Take</button>
          `;
          pane.appendChild(goldRow);

          for (const member of partyMembers) {
            const card = document.createElement('div');
            card.style.cssText = 'background:#0a1a2a;border:1px solid #1565c0;border-radius:4px;padding:6px 8px;margin:2px;min-width:220px;';
            const hpPct = member.max_hp > 0 ? Math.round(member.hp * 100 / member.max_hp) : 0;
            const hpColor = hpPct > 60 ? '#4caf50' : hpPct > 30 ? '#ff9800' : '#e94560';
            card.innerHTML = `
              <div style="font-weight:bold;color:#e0e0e0;margin-bottom:4px;">${esc(member.name)}</div>
              <div style="margin-bottom:4px;">
                <div style="background:#1a1a2e;border-radius:2px;height:14px;overflow:hidden;">
                  <div style="background:${hpColor};height:100%;width:${hpPct}%;transition:width 0.3s;"></div>
                </div>
                <span style="font-size:11px;color:#888;">HP ${member.hp}/${member.max_hp} (${hpPct}%)</span>
              </div>
              <div style="display:flex;gap:2px;flex-wrap:wrap;margin-bottom:4px;">
                <button class="hbtn" style="border-color:#388e3c;color:#90e898;" onclick="modHp('${member.id}',5)">+5</button>
                <button class="hbtn" style="border-color:#c62828;color:#e89090;" onclick="modHp('${member.id}',-5)">-5</button>
                <button class="hbtn" style="border-color:#388e3c;color:#90e898;" onclick="modHp('${member.id}','full')">Full</button>
              </div>
              <div style="display:flex;gap:2px;flex-wrap:wrap;">
                ${STATUS_CONDITIONS.map(c => {
                  const has = (member.conditions || []).includes(c);
                  return `<button class="hbtn" style="font-size:10px;padding:1px 5px;border-color:${has ? '#e94560' : '#334'};color:${has ? '#e94560' : '#666'};" onclick="toggleStatus('${member.id}','${c}')" title="${c}">${c.slice(0,3)}</button>`;
                }).join('')}
              </div>
            `;
            pane.appendChild(card);
          }
        }

        function modHp(memberId, amount) {
          if (amount === 'full') sendRaw(JSON.stringify({cmd:'modify_hp',member:memberId,mode:'full'}));
          else sendRaw(JSON.stringify({cmd:'modify_hp',member:memberId,amount}));
          setTimeout(fetchPartyAndBuild, 300);
        }

        function toggleStatus(memberId, condition) {
          sendRaw(JSON.stringify({cmd:'modify_status',member:memberId,condition,operation:'toggle'}));
          setTimeout(fetchPartyAndBuild, 300);
        }

        function addGold(sign) {
          const amt = parseInt(document.getElementById('gold-amount')?.value || '100') * sign;
          sendRaw(JSON.stringify({cmd:'modify_gold', amount: amt}));
          setTimeout(() => sendRaw(JSON.stringify({cmd:'get_party'})), 350);
        }

        function buildItemPicker(categories) {
          itemCategories = categories || [];
          const panesEl = document.getElementById('hpanes');
          let pane = panesEl.querySelector('[data-pane="Equip"]');
          if (!pane) {
            pane = document.createElement('div');
            pane.className = 'hpane';
            pane.dataset.pane = 'Equip';
            panesEl.appendChild(pane);
          }
          pane.innerHTML = '';

          if (!partyMembers.length) {
            pane.innerHTML = '<span style="color:#888;padding:4px;">Start a game first.</span>';
            return;
          }

          const charSelect = document.createElement('div');
          charSelect.className = 'equip-char-select';
          for (const m of partyMembers) {
            const b = document.createElement('button');
            b.className = 'hbtn';
            b.textContent = m.name;
            b.style.borderColor = selectedEquipChar === m.id ? '#2196f3' : '#334';
            b.onclick = () => { selectedEquipChar = m.id; buildItemPicker(categories); };
            charSelect.appendChild(b);
          }
          pane.appendChild(charSelect);

          if (!itemCategories.length) {
            pane.innerHTML += '<span style="color:#888;">No items loaded.</span>';
            return;
          }

          const catBar = document.createElement('div');
          catBar.style.cssText = 'width:100%;display:flex;flex-wrap:wrap;gap:2px;margin-bottom:4px;';
          let activeCat = itemCategories[0]?.type;
          catBar.innerHTML = '';
          for (const cat of itemCategories) {
            const b = document.createElement('button');
            b.className = 'item-cat-btn';
            b.textContent = cat.label;
            b.title = cat.type;
            b.onclick = () => {
              activeCat = cat.type;
              catBar.querySelectorAll('.item-cat-btn').forEach(x => x.classList.remove('active'));
              b.classList.add('active');
              renderItemsGrid(cat);
            };
            catBar.appendChild(b);
          }
          pane.appendChild(catBar);
          catBar.querySelector('.item-cat-btn')?.classList.add('active');

          const grid = document.createElement('div');
          grid.className = 'equip-grid';
          pane.appendChild(grid);

          function renderItemsGrid(cat) {
            grid.innerHTML = '';
            for (const item of cat.items) {
              const row = document.createElement('div');
              row.className = 'item-row';
              const info = [item.name, item.damage ? `DMG:${item.damage}` : '', item.protection ? `PRO:${item.protection}` : '', item.charges ? `⚡${item.charges}` : ''].filter(Boolean).join(' ');
              row.textContent = info;
              row.title = `${item.name} | Weight: ${item.weight}g | Value: ${item.value}${item.charges ? ' | Charges: '+item.charges : ''}${item.spell && item.spell !== 'None' ? ' | Spell: '+item.spell : ''}`;
              row.onclick = () => {
                const slot = getItemSlot(cat.type);
                sendRaw(JSON.stringify({cmd:'equip_item',member:selectedEquipChar,item:item.id,slot}));
              };
              grid.appendChild(row);
            }
          }

          renderItemsGrid(itemCategories.find(c => c.type === activeCat) || itemCategories[0]);
        }

        function getItemSlot(itemType) {
          const slotMap = {
            'CloseRangeWeapon': 'RightHand', 'LongRangeWeapon': 'RightHand',
            'Armor': 'Chest', 'Helmet': 'Head', 'Shoes': 'Feet', 'Shield': 'LeftHand',
            'Amulet': 'Neck', 'MagicRing': 'RightFinger',
          };
          return slotMap[itemType] || 'RightHand';
        }

        function buildEncounterList(encounters) {
          allEncounters = encounters || [];
          csBuildEncounterPicker(); // update Combat Suite picker if open
          const panesEl = document.getElementById('hpanes');
          let pane = panesEl.querySelector('[data-pane="Combat"]');
          if (!pane) {
            pane = document.createElement('div');
            pane.className = 'hpane';
            pane.dataset.pane = 'Combat';
            panesEl.appendChild(pane);
          }
          pane.innerHTML = '';

          const wrap = document.createElement('div');
          wrap.className = 'encounter-scroll';

          for (const enc of encounters) {
            const b = document.createElement('button');
            b.className = 'hbtn';
            b.textContent = enc.name;
            b.title = enc.id;
            b.style.cssText = 'width:100%;text-align:left;font-size:10px;';
            b.onclick = () => sendRaw(JSON.stringify({cmd:'raise_event',event:`encounter ${enc.id} CombatBackground.Dungeon`}));
            wrap.appendChild(b);
          }
          pane.appendChild(wrap);
        }

        function buildMerchantList() {
          const panesEl = document.getElementById('hpanes');
          let pane = panesEl.querySelector('[data-pane="Merchant"]');
          if (!pane) {
            pane = document.createElement('div');
            pane.className = 'hpane';
            pane.dataset.pane = 'Merchant';
            panesEl.appendChild(pane);
          }
          pane.innerHTML = '';

          const charRow = document.createElement('div');
          charRow.style.cssText = 'display:flex;align-items:center;gap:4px;margin-bottom:6px;';
          const charLabel = document.createElement('span');
          charLabel.textContent = 'Member:';
          charLabel.style.cssText = 'color:#aaa;font-size:10px;white-space:nowrap;';
          const charSel = document.createElement('select');
          charSel.style.cssText = 'background:#0a1a2a;color:#e0e0e0;border:1px solid #1565c0;flex:1;font-size:10px;';
          if (partyMembers.length === 0) {
            const opt = document.createElement('option');
            opt.value = 'PartyMember.None';
            opt.textContent = '(none)';
            charSel.appendChild(opt);
          } else {
            for (const m of partyMembers) {
              const opt = document.createElement('option');
              opt.value = m.id;
              opt.textContent = m.name;
              charSel.appendChild(opt);
            }
          }
          selectedMerchantChar = charSel.value;
          charSel.onchange = () => { selectedMerchantChar = charSel.value; };
          charRow.appendChild(charLabel);
          charRow.appendChild(charSel);
          pane.appendChild(charRow);

          function addSection(title, merchants) {
            const hdr = document.createElement('div');
            hdr.style.cssText = 'color:#ff9800;font-size:10px;margin:6px 0 2px;padding-left:2px;';
            hdr.textContent = '── ' + title + ' ──';
            pane.appendChild(hdr);
            for (const [id, label] of merchants) {
              const b = document.createElement('button');
              b.className = 'hbtn';
              b.textContent = label;
              b.title = id;
              b.style.cssText = 'width:100%;text-align:left;font-size:10px;';
              b.onclick = () => sendRaw(JSON.stringify({cmd:'enter_merchant', merchant_id: id, member: selectedMerchantChar || 'PartyMember.None'}));
              pane.appendChild(b);
            }
          }

          addSection('Toronto', [
            ['Merchant.Wania','Wania'],['Merchant.Rejira','Rejira'],['Merchant.Snird','Snird'],
            ['Merchant.Rabir','Rabir'],['Merchant.Krinn','Krinn'],['Merchant.Rifrako','Rifrako'],
            ['Merchant.Tamno','Tamno'],['Merchant.Winion','Winion'],
            ['Merchant.RovesSpells','Roves Spells'],['Merchant.AltheaSpells','Althea Spells'],
          ]);
          addSection('Jirinaar', [
            ['Merchant.Zeibe','Zeibe'],['Merchant.JerosFilled','Jeros (Filled)'],
            ['Merchant.Mokonou','Mokonou'],['Merchant.Nadje','Nadje'],['Merchant.Posch','Posch'],
            ['Merchant.Bagga','Bagga'],['Merchant.RioleaFilled','Riolea (Filled)'],
            ['Merchant.KounosTrader','Kounos Trader'],['Merchant.Edjirr','Edjirr'],
            ['Merchant.RioleaEmpty','Riolea (Empty)'],['Merchant.JerosEmpty','Jeros (Empty)'],
          ]);
        }

        (function buildHotkeyTabs() {
          const tabsEl = document.getElementById('htabs');
          const panesEl = document.getElementById('hpanes');
          let first = true;
          for (const [mode, actions] of Object.entries(HOTKEYS)) {
            const tab = document.createElement('button');
            tab.className = 'htab' + (first ? ' active' : '');
            tab.textContent = mode;
            tab.dataset.tab = mode;
            tabsEl.appendChild(tab);

            const pane = document.createElement('div');
            pane.className = 'hpane' + (first ? ' active' : '');
            pane.dataset.pane = mode;
            if (mode === 'Characters' || mode === 'Equip' || mode === 'Combat' || mode === 'Merchant' || mode === 'Sequences' || mode === 'AutoCombat') {
              pane.innerHTML = `<span style="color:#888;padding:4px;">Loading…</span>`;
            } else {
              for (const [action, label, cont] of actions) {
                const b = document.createElement('button');
                b.className = 'hbtn';
                if (cont) b.dataset.cont = '1';
                b.title = action;
                b.textContent = label;
                if (mode === 'Overlays') {
                  b.onclick = () => sendRaw(JSON.stringify({cmd: action}));
                } else {
                  b.onclick = () => sendRaw(JSON.stringify({cmd:'send_input_action', action}));
                }
                pane.appendChild(b);
              }
            }
            panesEl.appendChild(pane);

            tab.onclick = () => {
              tabsEl.querySelectorAll('.htab').forEach(t => t.classList.remove('active'));
              panesEl.querySelectorAll('.hpane').forEach(p => p.classList.remove('active'));
              tab.classList.add('active');
              pane.classList.add('active');
              if (mode === 'Characters' && !charsTabBuilt) { charsTabBuilt = true; fetchPartyWithRetry(); }
              if (mode === 'Equip' && !equipTabBuilt) { equipTabBuilt = true; if (!charsTabBuilt) { charsTabBuilt = true; fetchPartyWithRetry(); } fetchItems(); }
              if (mode === 'Combat' && !combatTabBuilt) { combatTabBuilt = true; fetchEncounters(); }
              if (mode === 'Merchant' && !merchantTabBuilt) { merchantTabBuilt = true; if (!charsTabBuilt) { charsTabBuilt = true; fetchPartyAndBuild(); } buildMerchantList(); }
              if (mode === 'Sequences' && !sequencesTabBuilt) { sequencesTabBuilt = true; buildSequencesPane(); }
              if (mode === 'AutoCombat' && !autoCombatTabBuilt) { autoCombatTabBuilt = true; buildAutoCombatPane(); }
            };
            first = false;
          }
        })();

        // ── Sequence execution engine ─────────────────────────────────────────
        function sleep(ms) { return new Promise(r => setTimeout(r, ms)); }

        function waitForGameEvent(type, timeout_ms = 60000) {
          return new Promise((resolve, reject) => {
            const timer = setTimeout(() => {
              pendingEventWaiters = pendingEventWaiters.filter(w => w.resolve !== resolve);
              reject(new Error('timeout:' + type));
            }, timeout_ms);
            pendingEventWaiters.push({ type, resolve, reject, timer });
          });
        }

        async function sendCmdAsync(payload) {
          return new Promise(resolve => {
            const ws = new WebSocket('ws://' + location.host + '/agent');
            ws.onopen  = () => ws.send(typeof payload === 'string' ? payload : JSON.stringify(payload));
            ws.onmessage = e => { ws.close(); try { resolve(JSON.parse(e.data)); } catch { resolve({}); } };
            ws.onerror  = () => resolve({ ok: false, error: 'ws_error' });
          });
        }

        async function waitForCondition(step) {
          const timeout = step.timeout_ms || 30000;
          const start = Date.now();
          while (Date.now() - start < timeout) {
            if (seqAbort) return false;
            if (step.condition === 'delay') { await sleep(step.ms || 1000); return true; }
            if (step.condition === 'combat_ended') {
              try { await waitForGameEvent('combat_ended', timeout - (Date.now() - start)); return true; }
              catch { return false; }
            }
            let r;
            if (step.condition === 'scene')           r = await sendCmdAsync({cmd:'get_scene'});
            else if (step.condition === 'combat_planning' || step.condition === 'no_combat')
              r = await sendCmdAsync({cmd:'get_combat'});
            else { await sleep(200); continue; }
            if (step.condition === 'scene' && r.ok && r.result?.scene_id === step.value) return true;
            if (step.condition === 'combat_planning' && r.ok && r.result?.planning_state === 'Planning') return true;
            if (step.condition === 'no_combat' && r.ok && r.result?.planning_state === 'NotInCombat') return true;
            await sleep(250);
          }
          return false; // timed out
        }

        async function executeFightStep(step) {
          const enc  = step.encounter  || 'MonsterGroup.DebugMix';
          const bg   = step.background || 'CombatBackground.Dungeon';
          const name = step.name || enc.replace('MonsterGroup.','');

          const r = await sendCmdAsync({ cmd: 'raise_event', event: `encounter ${enc} ${bg}` });
          if (!r.ok) { seqResults.push({ name, result: 'FailedToStart', ok: false }); renderSuiteResults(); return false; }

          const started = await waitForCondition({ condition: 'combat_planning', timeout_ms: 15000 });
          if (!started) { seqResults.push({ name, result: 'DidNotStart', ok: false }); renderSuiteResults(); return false; }

          let combatResult = 'Timeout';
          try {
            const evt = await waitForGameEvent('combat_ended', step.timeout_ms || 180000);
            combatResult = evt.result || 'Unknown';
          } catch { /* timeout */ }

          const ok = combatResult === 'Victory';
          seqResults.push({ name, result: combatResult, ok });
          renderSuiteResults();

          if (!ok) return false; // party killed or timed out → stop

          await sleep(800); // brief pause between fights
          return true;
        }

        function renderSuiteResults() {
          const el = document.getElementById('cs-results');
          if (!el) return;
          const rows = seqResults.map(r => {
            const color = r.ok ? '#4caf50' : r.result === 'Retreat' ? '#ff9800' : '#e94560';
            return `<div style="display:flex;gap:6px;align-items:center;font-size:11px;padding:1px 0;">
              <span style="color:${color};font-weight:bold;min-width:12px;">${r.ok ? '✓' : '✗'}</span>
              <span style="flex:1;color:#e0e0e0;">${esc(r.name)}</span>
              <span style="color:${color};">${esc(r.result)}</span>
            </div>`;
          }).join('');
          const wins = seqResults.filter(r => r.ok).length;
          const defeat = seqResults.find(r => !r.ok);
          const summaryColor = defeat ? '#e94560' : wins > 0 ? '#4caf50' : '#888';
          const summary = seqResults.length > 0
            ? `<div style="margin-top:4px;padding-top:4px;border-top:1px solid #334;color:${summaryColor};font-size:11px;font-weight:bold;">
                ${wins}/${seqResults.length} won${defeat ? ` — stopped: ${esc(defeat.result)} at "${esc(defeat.name)}"` : ' — all clear! ✓'}
              </div>` : '';
          el.innerHTML = `<div style="margin-top:6px;">${rows}${summary}</div>`;
          // Also add summary to log when complete
          if (defeat || wins === seqResults.length)
            addRow(defeat ? 'err' : 'resp', '🏟', `Suite: ${wins}/${seqResults.length} victories${defeat ? ' — Party defeated at "'+defeat.name+'"' : ''}`);
        }

        async function executeSequence(seq) {
          if (seqRunning) { addRow('info','⚡','Sequence already running'); return; }
          seqRunning = true; seqAbort = false;
          seqResults = [];
          const statusEl = document.getElementById('seq-run-status');
          const steps = seq.steps || [];
          for (let i = 0; i < steps.length; i++) {
            if (seqAbort) break;
            const step = steps[i];
            if (statusEl) statusEl.textContent = `Step ${i+1}/${steps.length}: ${step.type} ${step.cmd || step.condition || step.encounter || ''}`;
            if (step.type === 'cmd') {
              const payload = { ...(step.params || {}), cmd: step.cmd };
              const r = await sendCmdAsync(payload);
              if (!r.ok) { addRow('err','⚡',`Seq step ${i+1} failed: ${r.error}`); }
            } else if (step.type === 'wait') {
              const ok = await waitForCondition(step);
              if (!ok) addRow('info','⚡',`Seq step ${i+1} wait timed out`);
            } else if (step.type === 'fight') {
              const ok = await executeFightStep(step);
              if (!ok) { seqAbort = true; break; }
            }
          }
          const wasAborted = seqAbort;
          seqRunning = false; seqAbort = false;
          if (statusEl) statusEl.textContent = wasAborted ? '⛔ Stopped' : '✓ Done';
          if (seqResults.length === 0)
            addRow(wasAborted ? 'err' : 'resp', '⚡', `Sequence "${seq.name}" ${wasAborted ? 'aborted' : 'completed'}`);
        }

        // ── Sequence step metadata ────────────────────────────────────────────
        const SEQ_COMMANDS = [
          { cmd: 'start_new_game',    label: 'Start New Game',      params: [] },
          { cmd: 'quicksave',         label: 'Quicksave',           params: [] },
          { cmd: 'quickload',         label: 'Quickload',           params: [] },
          { cmd: 'dismiss_message',   label: 'Dismiss Message',     params: [] },
          { cmd: 'auto_combat_start', label: 'Auto-Combat Start',   params: [{ key: 'attack_priority', ph: 'weakest / nearest / strongest' }] },
          { cmd: 'auto_combat_stop',  label: 'Auto-Combat Stop',    params: [] },
          { cmd: 'equip_item',        label: 'Equip Item',          params: [{ key: 'member', ph: 'Tom' }, { key: 'item', ph: 'Sword' }, { key: 'slot', ph: 'RightHand' }] },
          { cmd: 'modify_gold',       label: 'Modify Gold',         params: [{ key: 'amount', ph: '100 (negative to take)' }] },
          { cmd: 'modify_hp',         label: 'Modify HP',           params: [{ key: 'member', ph: 'Tom' }, { key: 'amount', ph: '10 or full' }] },
          { cmd: 'raise_event',       label: 'Raise Event',         params: [{ key: 'event', ph: 'encounter MonsterGroup.TwoSkrinn1 CombatBackground.Dungeon' }] },
          { cmd: 'teleport',          label: 'Teleport',            params: [{ key: 'map', ph: 'Map.TorontoBegin' }, { key: 'x', ph: '31' }, { key: 'y', ph: '76' }] },
          { cmd: 'respond',           label: 'Dialog: Respond',     params: [{ key: 'option', ph: '1' }] },
          { cmd: 'save_game',         label: 'Save Game',           params: [{ key: 'id', ph: '1' }, { key: 'name', ph: 'AgentSave' }] },
          { cmd: 'load_game',         label: 'Load Game',           params: [{ key: 'id', ph: '1' }] },
          { cmd: 'load_map',          label: 'Load Map',            params: [{ key: 'map', ph: 'Map.TorontoBegin' }] },
        ];
        const SEQ_CONDITIONS = [
          { value: 'delay',            label: 'Delay (ms)',               extra: 'ms' },
          { value: 'scene',            label: 'Wait for Scene',           extra: 'value' },
          { value: 'combat_planning',  label: 'Wait: Combat Planning',    extra: null },
          { value: 'no_combat',        label: 'Wait: Combat Ends',        extra: null },
          { value: 'combat_ended',     label: 'Wait: Combat Result Event',extra: null },
        ];

        function addSeqStep(type) {
          if (!editingSequence) return;
          const adder = document.getElementById('seq-step-adder');
          if (!adder) return;
          adder.innerHTML = '';
          adder.style.display = 'block';

          if (type === 'cmd') {
            const sel = document.createElement('select');
            sel.style.cssText = 'background:#0a1a2a;color:#e0e0e0;border:1px solid #1565c0;padding:2px;font-size:11px;margin-bottom:4px;width:100%;';
            for (const c of SEQ_COMMANDS) {
              const opt = document.createElement('option');
              opt.value = c.cmd; opt.textContent = c.label;
              sel.appendChild(opt);
            }
            const paramsDiv = document.createElement('div');
            paramsDiv.id = 'seq-adder-params';
            paramsDiv.style.cssText = 'display:flex;flex-direction:column;gap:2px;margin-bottom:4px;';
            function renderCmdParams() {
              paramsDiv.innerHTML = '';
              const cmd = SEQ_COMMANDS.find(c => c.cmd === sel.value);
              if (!cmd || !cmd.params.length) return;
              for (const p of cmd.params) {
                const row = document.createElement('div');
                row.style.cssText = 'display:flex;gap:4px;align-items:center;';
                row.innerHTML = `<label style="color:#aaa;font-size:10px;min-width:60px;">${esc(p.key)}</label><input id="seq-param-${esc(p.key)}" placeholder="${esc(p.ph)}" style="flex:1;background:#0a0a1a;border:1px solid #334;color:#e0e0e0;padding:2px 5px;border-radius:3px;font-size:11px;font-family:inherit;">`;
                paramsDiv.appendChild(row);
              }
            }
            sel.onchange = renderCmdParams;
            adder.appendChild(sel);
            adder.appendChild(paramsDiv);
            renderCmdParams();
            const btnRow = document.createElement('div');
            btnRow.style.cssText = 'display:flex;gap:3px;';
            btnRow.innerHTML = `<button class="hbtn" style="border-color:#4caf50;color:#90e898;" onclick="confirmAddCmdStep()">+ Add</button><button class="hbtn" onclick="document.getElementById('seq-step-adder').style.display='none'">Cancel</button>`;
            adder.appendChild(btnRow);

          } else {
            const sel = document.createElement('select');
            sel.style.cssText = 'background:#0a1a2a;color:#e0e0e0;border:1px solid #1565c0;padding:2px;font-size:11px;margin-bottom:4px;width:100%;';
            for (const c of SEQ_CONDITIONS) {
              const opt = document.createElement('option'); opt.value = c.value; opt.textContent = c.label;
              sel.appendChild(opt);
            }
            const extraDiv = document.createElement('div');
            extraDiv.id = 'seq-adder-extra';
            extraDiv.style.cssText = 'margin-bottom:4px;';
            function renderWaitExtra() {
              extraDiv.innerHTML = '';
              const cond = SEQ_CONDITIONS.find(c => c.value === sel.value);
              if (!cond?.extra) return;
              extraDiv.innerHTML = `<input id="seq-adder-extra-val" placeholder="${cond.extra === 'ms' ? '1000' : 'expected value'}" style="background:#0a0a1a;border:1px solid #334;color:#e0e0e0;padding:2px 5px;border-radius:3px;font-size:11px;font-family:inherit;width:100%;">`;
            }
            sel.onchange = renderWaitExtra;
            adder.appendChild(sel);
            adder.appendChild(extraDiv);
            renderWaitExtra();
            const btnRow = document.createElement('div');
            btnRow.style.cssText = 'display:flex;gap:3px;';
            btnRow.innerHTML = `<button class="hbtn" style="border-color:#ff9800;color:#ffd080;" onclick="confirmAddWaitStep()">+ Add</button><button class="hbtn" onclick="document.getElementById('seq-step-adder').style.display='none'">Cancel</button>`;
            adder.appendChild(btnRow);
          }
        }

        function confirmAddCmdStep() {
          if (!editingSequence) return;
          const sel = document.querySelector('#seq-step-adder select');
          if (!sel) return;
          const cmd = sel.value;
          const cmdMeta = SEQ_COMMANDS.find(c => c.cmd === cmd);
          const params = {};
          if (cmdMeta) {
            for (const p of cmdMeta.params) {
              const inp = document.getElementById('seq-param-' + p.key);
              const v = inp?.value?.trim();
              if (v) params[p.key] = isNaN(v) || v === '' ? v : Number(v);
            }
          }
          editingSequence.steps.push({ type: 'cmd', cmd, ...(Object.keys(params).length ? { params } : {}) });
          document.getElementById('seq-step-adder').style.display = 'none';
          renderSeqEditorSteps();
        }

        function confirmAddWaitStep() {
          if (!editingSequence) return;
          const sel = document.querySelector('#seq-step-adder select');
          if (!sel) return;
          const condition = sel.value;
          const step = { type: 'wait', condition };
          const extraInp = document.getElementById('seq-adder-extra-val');
          if (extraInp?.value?.trim()) {
            const cond = SEQ_CONDITIONS.find(c => c.value === condition);
            if (cond?.extra === 'ms') step.ms = parseInt(extraInp.value) || 1000;
            else if (cond?.extra === 'value') step.value = extraInp.value.trim();
          }
          editingSequence.steps.push(step);
          document.getElementById('seq-step-adder').style.display = 'none';
          renderSeqEditorSteps();
        }

        // ── Sequences pane ────────────────────────────────────────────────────
        function buildSequencesPane() {
          const panesEl = document.getElementById('hpanes');
          let pane = panesEl.querySelector('[data-pane="Sequences"]');
          if (!pane) {
            pane = document.createElement('div');
            pane.className = 'hpane';
            pane.dataset.pane = 'Sequences';
            panesEl.appendChild(pane);
          }
          pane.innerHTML = '';

          // Toolbar
          const toolbar = document.createElement('div');
          toolbar.style.cssText = 'display:flex;gap:4px;margin-bottom:6px;align-items:center;flex-wrap:wrap;';
          toolbar.innerHTML = `
            <button class="hbtn" style="border-color:#4caf50;color:#90e898;" onclick="fetchAndRenderSequences()">⟳ Refresh</button>
            <button class="hbtn" style="border-color:#2196f3;color:#90c4e8;" onclick="openSeqEditor(null)">+ New</button>
            <button class="hbtn" style="border-color:#e94560;color:#e89090;" onclick="seqAbort=true" id="seq-abort-btn">⛔ Abort</button>
            <span id="seq-run-status" style="color:#888;font-size:11px;padding:2px 6px;"></span>`;
          pane.appendChild(toolbar);

          const listEl = document.createElement('div');
          listEl.id = 'seq-list';
          listEl.style.cssText = 'display:flex;flex-direction:column;gap:3px;max-height:240px;overflow-y:auto;';
          pane.appendChild(listEl);

          // Editor (hidden by default)
          const editor = document.createElement('div');
          editor.id = 'seq-editor';
          editor.style.cssText = 'display:none;margin-top:6px;border:1px solid #1565c0;border-radius:4px;padding:8px;background:#0a1220;';
          editor.innerHTML = `
            <div style="margin-bottom:4px;display:flex;gap:4px;align-items:center;">
              <input id="seq-edit-name" placeholder="Sequence name" style="flex:1;background:#0a0a1a;border:1px solid #334;color:#e0e0e0;padding:3px 6px;border-radius:3px;font-family:inherit;font-size:12px;">
              <input id="seq-edit-desc" placeholder="Description" style="flex:2;background:#0a0a1a;border:1px solid #334;color:#e0e0e0;padding:3px 6px;border-radius:3px;font-family:inherit;font-size:12px;">
            </div>
            <div id="seq-edit-steps" style="display:flex;flex-direction:column;gap:2px;max-height:160px;overflow-y:auto;margin-bottom:4px;"></div>
            <div id="seq-step-adder" style="display:none;background:#0a0a2a;border:1px solid #1565c0;border-radius:3px;padding:6px;margin-bottom:4px;"></div>
            <div style="display:flex;gap:3px;flex-wrap:wrap;">
              <button class="hbtn" style="border-color:#4caf50;" onclick="addSeqStep('cmd')">+ Command</button>
              <button class="hbtn" style="border-color:#ff9800;" onclick="addSeqStep('wait')">+ Wait</button>
              <button class="hbtn" style="border-color:#4caf50;color:#90e898;" onclick="saveEditingSequence()">💾 Save</button>
              <button class="hbtn" onclick="closeSeqEditor()">Cancel</button>
            </div>`;
          pane.appendChild(editor);

          fetchAndRenderSequences();
        }

        function fetchAndRenderSequences() {
          fetch('/sequences').then(r => r.json()).then(seqs => {
            const listEl = document.getElementById('seq-list');
            if (!listEl) return;
            listEl.innerHTML = '';
            if (!seqs.length) { listEl.innerHTML = '<span style="color:#888;font-size:11px;padding:4px;">No sequences saved.</span>'; return; }
            for (const s of seqs) {
              const row = document.createElement('div');
              row.style.cssText = 'display:flex;gap:3px;align-items:center;background:#0a1220;border:1px solid #1a2a3a;border-radius:3px;padding:3px 6px;';
              row.innerHTML = `
                <span style="flex:1;font-size:11px;color:#e0e0e0;" title="${esc(s.description)}">${esc(s.name)}</span>
                <span style="color:#888;font-size:10px;">${s.steps} steps</span>
                <button class="hbtn" style="border-color:#4caf50;color:#90e898;font-size:10px;" onclick="runSequenceFile('${esc(s.file)}')">▶ Run</button>
                <button class="hbtn" style="font-size:10px;" onclick="editSequenceFile('${esc(s.file)}')">✏️</button>
                <button class="hbtn" style="border-color:#e94560;color:#e89090;font-size:10px;" onclick="deleteSequence('${esc(s.file)}')">🗑</button>`;
              listEl.appendChild(row);
            }
          }).catch(() => {});
        }

        function runSequenceFile(file) {
          fetch('/sequences?file=' + encodeURIComponent(file))
            .then(r => r.ok ? r.json() : null)
            .catch(() => null)
            .then(seq => { if (seq) executeSequence(seq); else addRow('err','⚡','Could not load sequence: ' + file); });
        }

        function editSequenceFile(file) {
          fetch('/sequences?file=' + encodeURIComponent(file))
            .then(r => r.ok ? r.json() : null)
            .catch(() => null)
            .then(seq => { if (seq) openSeqEditor(seq); });
        }

        function deleteSequence(file) {
          if (!confirm('Delete sequence "' + file + '"?')) return;
          fetch('/sequences?file=' + encodeURIComponent(file), { method: 'DELETE' })
            .then(() => fetchAndRenderSequences());
        }

        function openSeqEditor(seq) {
          editingSequence = seq ? JSON.parse(JSON.stringify(seq)) : { name: '', description: '', steps: [] };
          document.getElementById('seq-editor').style.display = 'block';
          document.getElementById('seq-edit-name').value = editingSequence.name || '';
          document.getElementById('seq-edit-desc').value = editingSequence.description || '';
          renderSeqEditorSteps();
        }

        function closeSeqEditor() {
          editingSequence = null;
          document.getElementById('seq-editor').style.display = 'none';
        }

        function renderSeqEditorSteps() {
          const el = document.getElementById('seq-edit-steps');
          if (!el || !editingSequence) return;
          el.innerHTML = '';
          (editingSequence.steps || []).forEach((step, i) => {
            const row = document.createElement('div');
            row.style.cssText = 'display:flex;gap:2px;align-items:center;background:#0a0a1a;border:1px solid #223;border-radius:2px;padding:2px 4px;';
            const label = step.type === 'cmd'
              ? `<b style="color:#2196f3">cmd</b> ${esc(step.cmd || '')} ${step.params ? '<span style="color:#888">'+esc(JSON.stringify(step.params))+'</span>' : ''}`
              : `<b style="color:#ff9800">wait</b> ${esc(step.condition || '')}${step.value ? ' = '+esc(step.value) : ''}${step.ms ? ' '+step.ms+'ms' : ''}`;
            row.innerHTML = `
              <span style="flex:1;font-size:11px;">${label}</span>
              <button class="hbtn" style="font-size:10px;padding:1px 5px;" onclick="moveStep(${i},-1)">↑</button>
              <button class="hbtn" style="font-size:10px;padding:1px 5px;" onclick="moveStep(${i},1)">↓</button>
              <button class="hbtn" style="border-color:#e94560;color:#e89090;font-size:10px;padding:1px 5px;" onclick="removeStep(${i})">✕</button>`;
            el.appendChild(row);
          });
        }

        function moveStep(i, dir) {
          const steps = editingSequence.steps;
          const j = i + dir;
          if (j < 0 || j >= steps.length) return;
          [steps[i], steps[j]] = [steps[j], steps[i]];
          renderSeqEditorSteps();
        }

        function removeStep(i) { editingSequence.steps.splice(i, 1); renderSeqEditorSteps(); }


        function saveEditingSequence() {
          if (!editingSequence) return;
          editingSequence.name = document.getElementById('seq-edit-name').value.trim();
          editingSequence.description = document.getElementById('seq-edit-desc').value.trim();
          if (!editingSequence.name) { alert('Name required'); return; }
          fetch('/sequences', { method: 'POST', headers: {'Content-Type':'application/json'}, body: JSON.stringify(editingSequence) })
            .then(r => r.json())
            .then(() => { closeSeqEditor(); fetchAndRenderSequences(); })
            .catch(err => alert('Save failed: ' + err));
        }

        // ── Auto-Combat pane ──────────────────────────────────────────────────
        const CS_BACKGROUNDS = [
          'CombatBackground.Dungeon','CombatBackground.Forest','CombatBackground.Desert',
          'CombatBackground.Plains','CombatBackground.Beach','CombatBackground.Wasteland'
        ];

        function buildAutoCombatPane() {
          const panesEl = document.getElementById('hpanes');
          let pane = panesEl.querySelector('[data-pane="AutoCombat"]');
          if (!pane) {
            pane = document.createElement('div');
            pane.className = 'hpane';
            pane.dataset.pane = 'AutoCombat';
            panesEl.appendChild(pane);
          }
          pane.innerHTML = '';

          // ── Strategy section ──
          const stratDiv = document.createElement('div');
          stratDiv.style.cssText = 'padding:6px;max-width:420px;width:100%;';
          stratDiv.innerHTML = `
            <div style="color:#ff9800;font-size:11px;margin-bottom:6px;font-weight:bold;">Auto-Combat Strategy (C# engine)</div>
            <div style="display:flex;gap:4px;align-items:center;margin-bottom:8px;">
              <button class="hbtn" style="border-color:#4caf50;color:#90e898;" onclick="startAutoCombat()">▶ Enable</button>
              <button class="hbtn" style="border-color:#e94560;color:#e89090;" onclick="stopAutoCombat()">⏹ Disable</button>
              <button class="hbtn" onclick="refreshAutoCombatStatus()">⟳</button>
              <span id="ac-status" style="color:#888;font-size:11px;padding:2px 6px;"></span>
            </div>
            <div style="display:grid;grid-template-columns:130px 1fr;gap:4px 8px;align-items:center;font-size:12px;">
              <label style="color:#aaa;">Attack Priority</label>
              <select id="ac-priority" style="background:#0a1a2a;color:#e0e0e0;border:1px solid #1565c0;padding:2px;font-size:11px;">
                <option value="weakest">Weakest first</option>
                <option value="nearest">Nearest first</option>
                <option value="strongest">Strongest first</option>
              </select>
              <label style="color:#aaa;">Heal Threshold</label>
              <div style="display:flex;align-items:center;gap:4px;">
                <input id="ac-heal" type="range" min="0" max="1" step="0.05" value="0"
                  style="flex:1;" oninput="document.getElementById('ac-heal-val').textContent=Math.round(this.value*100)+'%'">
                <span id="ac-heal-val" style="color:#888;font-size:11px;min-width:32px;">0%</span>
              </div>
            </div>`;
          pane.appendChild(stratDiv);

          // ── Combat Suite section ──
          const suiteDiv = document.createElement('div');
          suiteDiv.style.cssText = 'padding:6px;max-width:420px;width:100%;border-top:1px solid #1565c0;margin-top:4px;';
          suiteDiv.innerHTML = `
            <div style="color:#ff9800;font-size:11px;margin-bottom:6px;font-weight:bold;">Combat Suite Runner</div>

            <div style="display:grid;grid-template-columns:130px 1fr;gap:4px 6px;align-items:center;font-size:11px;margin-bottom:6px;">
              <label style="color:#aaa;">Setup</label>
              <select id="cs-start" style="background:#0a1a2a;color:#e0e0e0;border:1px solid #1565c0;padding:2px;font-size:11px;">
                <option value="new_game">New Game (TorontoBegin)</option>
                <option value="quickload">Quickload</option>
              </select>
              <label style="color:#aaa;">Auto-Combat</label>
              <label style="font-size:10px;display:flex;align-items:center;gap:4px;color:#aaa;">
                <input type="checkbox" id="cs-autocombat" checked style="accent-color:#4caf50;"> Enable during suite
              </label>
            </div>

            <div style="color:#aaa;font-size:10px;margin-bottom:3px;">Items to equip before fights:</div>
            <div id="cs-equip-list" style="display:flex;flex-direction:column;gap:2px;margin-bottom:4px;max-height:80px;overflow-y:auto;"></div>
            <div style="display:flex;gap:3px;margin-bottom:6px;flex-wrap:wrap;">
              <input id="cs-equip-member" placeholder="Member (e.g. Tom)" style="width:90px;background:#0a0a1a;border:1px solid #334;color:#e0e0e0;padding:2px 5px;border-radius:3px;font-size:10px;font-family:inherit;">
              <input id="cs-equip-item" placeholder="Item (e.g. Sword)" style="width:110px;background:#0a0a1a;border:1px solid #334;color:#e0e0e0;padding:2px 5px;border-radius:3px;font-size:10px;font-family:inherit;">
              <select id="cs-equip-slot" style="background:#0a1a2a;color:#e0e0e0;border:1px solid #334;padding:2px;font-size:10px;">
                <option>RightHand</option><option>LeftHand</option><option>Chest</option>
                <option>Head</option><option>Feet</option><option>Neck</option>
              </select>
              <button class="hbtn" style="font-size:10px;border-color:#4caf50;" onclick="csAddEquip()">+ Add</button>
            </div>

            <div style="color:#aaa;font-size:10px;margin-bottom:3px;">Fight list (in order):</div>
            <div id="cs-fights-list" style="display:flex;flex-direction:column;gap:2px;max-height:160px;overflow-y:auto;margin-bottom:4px;"></div>
            <div style="display:flex;gap:3px;margin-bottom:6px;flex-wrap:wrap;align-items:center;">
              <select id="cs-enc-pick" style="flex:1;background:#0a1a2a;color:#e0e0e0;border:1px solid #1565c0;padding:2px;font-size:10px;min-width:140px;">
                <option value="">— pick encounter —</option>
              </select>
              <select id="cs-bg-pick" style="background:#0a1a2a;color:#e0e0e0;border:1px solid #1565c0;padding:2px;font-size:10px;">
                ${CS_BACKGROUNDS.map(b => `<option value="${b}">${b.replace('CombatBackground.','')}</option>`).join('')}
              </select>
              <button class="hbtn" style="font-size:10px;border-color:#4caf50;" onclick="csAddFight()">+ Add</button>
            </div>

            <div style="display:flex;gap:4px;margin-bottom:6px;flex-wrap:wrap;">
              <button class="hbtn" style="border-color:#4caf50;color:#90e898;" onclick="runCombatSuite()">▶ Run Suite</button>
              <button class="hbtn" style="border-color:#e94560;color:#e89090;" onclick="seqAbort=true">⛔ Stop</button>
              <button class="hbtn" style="font-size:10px;" onclick="csSaveAsSequence()">💾 Save as Sequence</button>
              <span id="cs-run-status" style="color:#888;font-size:11px;padding:2px 4px;"></span>
            </div>
            <div id="cs-results" style="font-size:11px;"></div>`;
          pane.appendChild(suiteDiv);

          refreshAutoCombatStatus();
          // Populate encounter picker (may already be loaded)
          csBuildEncounterPicker();
          renderCsEquipList();
          renderCsFightList();
        }

        function csBuildEncounterPicker() {
          const sel = document.getElementById('cs-enc-pick');
          if (!sel) return;
          // Remove all except placeholder
          while (sel.options.length > 1) sel.remove(1);
          if (!allEncounters.length) { fetchEncounters(); return; } // will re-trigger buildEncounterList
          for (const e of allEncounters) {
            const opt = document.createElement('option');
            opt.value = e.id;
            opt.textContent = e.name;
            sel.appendChild(opt);
          }
        }

        function renderCsEquipList() {
          const el = document.getElementById('cs-equip-list');
          if (!el) return;
          if (!csEquipItems.length) { el.innerHTML = '<span style="color:#666;font-size:10px;">No items.</span>'; return; }
          el.innerHTML = csEquipItems.map((it, i) =>
            `<div style="display:flex;gap:3px;align-items:center;font-size:10px;background:#0a0a1a;border:1px solid #223;border-radius:2px;padding:1px 4px;">
              <span style="flex:1;color:#aaa;">${esc(it.member)} ← ${esc(it.item)} [${esc(it.slot)}]</span>
              <button class="hbtn" style="font-size:9px;padding:0 4px;border-color:#e94560;color:#e89090;" onclick="csRemoveEquip(${i})">✕</button>
            </div>`
          ).join('');
        }

        function csAddEquip() {
          const member = (document.getElementById('cs-equip-member')?.value || '').trim();
          const item   = (document.getElementById('cs-equip-item')?.value || '').trim();
          const slot   = document.getElementById('cs-equip-slot')?.value || 'RightHand';
          if (!member || !item) { alert('Member and item are required'); return; }
          const fullMember = member.startsWith('PartyMember.') ? member : 'PartyMember.' + member;
          const fullItem   = item.startsWith('Item.') ? item : 'Item.' + item;
          csEquipItems.push({ member: fullMember, item: fullItem, slot });
          renderCsEquipList();
        }

        function csRemoveEquip(i) { csEquipItems.splice(i, 1); renderCsEquipList(); }

        function renderCsFightList() {
          const el = document.getElementById('cs-fights-list');
          if (!el) return;
          if (!csFights.length) { el.innerHTML = '<span style="color:#666;font-size:10px;">No fights added.</span>'; return; }
          el.innerHTML = csFights.map((f, i) =>
            `<div style="display:flex;gap:3px;align-items:center;font-size:10px;background:#0a0a1a;border:1px solid #223;border-radius:2px;padding:1px 4px;">
              <span style="color:#888;min-width:16px;">${i+1}.</span>
              <span style="flex:1;color:#aaa;">${esc(f.name || f.encounter.replace('MonsterGroup.',''))}</span>
              <span style="color:#666;">${esc(f.background.replace('CombatBackground.',''))}</span>
              <button class="hbtn" style="font-size:9px;padding:0 4px;" onclick="csMoveFight(${i},-1)">↑</button>
              <button class="hbtn" style="font-size:9px;padding:0 4px;" onclick="csMoveFight(${i},1)">↓</button>
              <button class="hbtn" style="font-size:9px;padding:0 4px;border-color:#e94560;color:#e89090;" onclick="csRemoveFight(${i})">✕</button>
            </div>`
          ).join('');
        }

        function csAddFight() {
          const enc = document.getElementById('cs-enc-pick')?.value;
          const bg  = document.getElementById('cs-bg-pick')?.value || 'CombatBackground.Dungeon';
          if (!enc) { alert('Select an encounter first'); return; }
          const name = allEncounters.find(e => e.id === enc)?.name || enc.replace('MonsterGroup.','');
          csFights.push({ encounter: enc, background: bg, name });
          renderCsFightList();
        }

        function csMoveFight(i, dir) {
          const j = i + dir;
          if (j < 0 || j >= csFights.length) return;
          [csFights[i], csFights[j]] = [csFights[j], csFights[i]];
          renderCsFightList();
        }

        function csRemoveFight(i) { csFights.splice(i, 1); renderCsFightList(); }

        function csBuildSequence() {
          const startType = document.getElementById('cs-start')?.value || 'new_game';
          const steps = [];

          // Setup step
          if (startType === 'new_game')   steps.push({ type: 'cmd', cmd: 'start_new_game' });
          else if (startType === 'quickload') steps.push({ type: 'cmd', cmd: 'quickload' });
          steps.push({ type: 'wait', condition: 'delay', ms: 2000 });

          // Equip items
          for (const it of csEquipItems)
            steps.push({ type: 'cmd', cmd: 'equip_item', params: { member: it.member, item: it.item, slot: it.slot } });

          // Enable auto-combat if checked
          if (document.getElementById('cs-autocombat')?.checked)
            steps.push({ type: 'cmd', cmd: 'auto_combat_start', params: { attack_priority: document.getElementById('ac-priority')?.value || 'weakest' } });

          // Fights
          for (const f of csFights) steps.push({ type: 'fight', ...f });

          // Disable auto-combat after suite
          if (document.getElementById('cs-autocombat')?.checked)
            steps.push({ type: 'cmd', cmd: 'auto_combat_stop' });

          return { name: 'Combat Suite ' + new Date().toLocaleTimeString(), steps };
        }

        async function runCombatSuite() {
          if (!csFights.length) { alert('Add at least one fight'); return; }
          document.getElementById('cs-results').innerHTML = '';
          const seq = csBuildSequence();
          const statusEl = document.getElementById('cs-run-status');
          // Mirror seq-run-status to cs-run-status
          const origStatus = document.getElementById('seq-run-status');
          await executeSequence(seq);
          if (statusEl && origStatus) statusEl.textContent = origStatus.textContent;
        }

        function csSaveAsSequence() {
          if (!csFights.length) { alert('Add at least one fight'); return; }
          const seq = csBuildSequence();
          seq.name = prompt('Sequence name:', seq.name) || seq.name;
          fetch('/sequences', { method:'POST', headers:{'Content-Type':'application/json'}, body: JSON.stringify(seq) })
            .then(r => r.json())
            .then(r => { addRow('resp','💾',`Saved as: ${r.saved}`); })
            .catch(err => alert('Save failed: ' + err));
        }

        async function startAutoCombat() {
          const priority  = document.getElementById('ac-priority')?.value || 'weakest';
          const threshold = parseFloat(document.getElementById('ac-heal')?.value || '0');
          const r = await sendCmdAsync({ cmd: 'auto_combat_start', attack_priority: priority, heal_threshold: threshold });
          updateAutoCombatStatus(r.ok ? { enabled: true, attack_priority: priority, heal_threshold: threshold } : null);
        }

        async function stopAutoCombat() {
          const r = await sendCmdAsync({ cmd: 'auto_combat_stop' });
          updateAutoCombatStatus(r.ok ? { enabled: false } : null);
        }

        async function refreshAutoCombatStatus() {
          const r = await sendCmdAsync({ cmd: 'auto_combat_status' });
          if (r.ok) updateAutoCombatStatus(r.result);
        }

        function updateAutoCombatStatus(s) {
          const el = document.getElementById('ac-status');
          if (!el || !s) return;
          el.textContent = s.enabled ? '● Active' : '○ Inactive';
          el.style.color = s.enabled ? '#4caf50' : '#888';
          if (s.attack_priority) {
            const sel = document.getElementById('ac-priority');
            if (sel) sel.value = s.attack_priority;
          }
          if (s.heal_threshold !== undefined) {
            const inp = document.getElementById('ac-heal');
            if (inp) { inp.value = s.heal_threshold; document.getElementById('ac-heal-val').textContent = Math.round(s.heal_threshold * 100) + '%'; }
          }
        }

        // Listen for auto_combat events from the server
        // (handled in the existing ws.onmessage by checking outer.type)

        connectEvents();
        </script>
        </body>

        </html>
        """;
}
