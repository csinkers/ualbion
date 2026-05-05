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
            "start_new_game"     => RaiseGameEvent("new_game Map.TorontoBegin 31 76"),
            "dismiss_message"    => RaiseGameEvent("dismiss_message"),
            "enter_merchant"     => EnterMerchant(node),
            _ => Error("unknown_command",
                $"Unknown command '{cmd}'. Available: ping, quit, get_scene, get_party, get_inventory, get_map, get_time, get_npcs, " +
                "get_combat, select_combat_action, select_combat_target, equip_item, save_game, load_game, teleport, load_map, " +
                "talk_npc, respond, modify_gold, modify_hp, modify_status, raise_event, send_input_action, start_new_game, dismiss_message, enter_merchant")
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

    string GetEncounters()
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
                try { name = assets.LoadStringSafe(d.Name, "English"); }
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

        IPlayer player = null;
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

        var name = s.StartsWith("PartyMember.", StringComparison.Ordinal) ? s.Substring(14) : s;

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
        return RaiseGameEvent($"modify_gold {amount}");
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
          ['Scene',       '{"cmd":"get_scene"}'],
          ['Party',       '{"cmd":"get_party"}'],
          ['Combat',      '{"cmd":"get_combat"}'],
          ['Round',       '{"cmd":"raise_event","event":"begin_combat_round"}'],
          ['Dismiss',     '{"cmd":"raise_event","event":"dismiss_message"}'],
        ];

        function connectEvents() {
          const ws = new WebSocket('ws://' + location.host + '/events');
          ws.onopen  = () => { dot.className = 'on'; addRow('info','●','Connected to /events'); };
          ws.onclose = () => { dot.className = ''; addRow('info','●','Disconnected — reconnecting…'); setTimeout(connectEvents, 2000); };
          ws.onerror = () => {};
          ws.onmessage = e => {
            try {
              const outer = JSON.parse(e.data);
              if (outer.type === 'response' && outer.payload) {
                const inner = JSON.parse(outer.payload);
                if (inner.result && inner.result.members) {
                  buildCharactersPane(inner.result.members);
                  if (merchantTabBuilt) buildMerchantList();
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
          Characters: [],
          Equip: [],
          Combat: [],
          Merchant: [],
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

        function fetchPartyAndBuild() { sendRaw(JSON.stringify({cmd:'get_party'})); }
        function fetchItems() { sendRaw(JSON.stringify({cmd:'get_items'})); }
        function fetchEncounters() { sendRaw(JSON.stringify({cmd:'get_encounters'})); }

        function buildCharactersPane(members) {
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
            if (mode === 'Characters' || mode === 'Equip' || mode === 'Combat' || mode === 'Merchant') {
              pane.innerHTML = `<span style="color:#888;padding:4px;">Loading…</span>`;
            } else {
              for (const [action, label, cont] of actions) {
                const b = document.createElement('button');
                b.className = 'hbtn';
                if (cont) b.dataset.cont = '1';
                b.title = action;
                b.textContent = label;
                b.onclick = () => sendRaw(JSON.stringify({cmd:'send_input_action', action}));
                pane.appendChild(b);
              }
            }
            panesEl.appendChild(pane);

            tab.onclick = () => {
              tabsEl.querySelectorAll('.htab').forEach(t => t.classList.remove('active'));
              panesEl.querySelectorAll('.hpane').forEach(p => p.classList.remove('active'));
              tab.classList.add('active');
              pane.classList.add('active');
              if (mode === 'Characters' && !charsTabBuilt) { charsTabBuilt = true; fetchPartyAndBuild(); }
              if (mode === 'Equip' && !equipTabBuilt) { equipTabBuilt = true; fetchItems(); }
              if (mode === 'Combat' && !combatTabBuilt) { combatTabBuilt = true; fetchEncounters(); }
              if (mode === 'Merchant' && !merchantTabBuilt) { merchantTabBuilt = true; if (!charsTabBuilt) { charsTabBuilt = true; fetchPartyAndBuild(); } buildMerchantList(); }
            };
            first = false;
          }
        })();

        connectEvents();
        </script>
        </body>

        </html>
        """;
}
