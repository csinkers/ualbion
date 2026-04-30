# Agent Mode

The Agent Bridge lets an external program (LLM agent, test script, or automation tool) observe and control the game in real time via WebSocket.

---

## Architecture

```
AgentBridgeComponent   (Component, IDisposable)
  — embedded HTTP/WebSocket server on port 7399
  — receives JSON commands on the game thread via a command queue
  — broadcasts game events (LogEvent) to all /events subscribers

CommandLineOptions     — --agent flag activates the bridge
AssetSystem            — registers AgentBridgeComponent when --agent is set
```

### Activation

Pass `--agent` when launching the game:

```
UAlbion.exe -d3d --agent
```

The monitor dashboard opens automatically in the default browser at `http://localhost:7399/monitor`.

---

## Endpoints

| URL | Protocol | Direction | Purpose |
|---|---|---|---|
| `ws://localhost:7399/agent` | WebSocket | bidirectional | Send JSON commands, receive JSON responses |
| `ws://localhost:7399/events` | WebSocket | server→client | Broadcast of all commands, responses, and game log events |
| `http://localhost:7399/monitor` | HTTP GET | server→client | Browser dashboard UI |
| `http://localhost:7399/scripts` | HTTP GET | server→client | List Python scripts in `test/` directory |
| `http://localhost:7399/run-script?file=<name>` | HTTP POST | client→server | Run a Python script; output streamed to `/events` |

---

## Command Protocol

Every command is a JSON object with a `"cmd"` field. Responses are JSON with `"ok": true/false`.

```json
// Request
{ "cmd": "ping" }

// Success response
{ "ok": true, "result": { "pong": true } }

// Error response
{ "ok": false, "error": "not_ready", "message": "No game loaded." }
```

Commands are queued from the WebSocket receiver thread and processed on the **game thread** during `BeginFrameEvent`, ensuring thread safety.

---

## Available Commands

### Game state

| Command | Parameters | Description |
|---|---|---|
| `ping` | — | Connectivity check. |
| `get_scene` | — | Returns `scene_id` (e.g. `"Map3D"`, `"Combat"`). |
| `get_map` | — | Returns `map_id`. |
| `get_time` | — | Returns `day`, `hour`, `minute`, `tick`. |
| `get_party` | — | Returns leader, gold, and full member list with HP/level/class/combat position. |
| `get_inventory` | — | Returns each member's equipped items and gold/rations. |
| `get_npcs` | — | Returns all NPCs on the current map with position and movement type. |
| `get_combat` | — | Returns current scene, party positions/HP, and the full tile map (30 tiles). |

### Navigation

| Command | Parameters | Description |
|---|---|---|
| `teleport` | `map`, `x`, `y`, `direction` (optional) | Teleport the party. |
| `load_map` | `map` | Load a map by ID string (e.g. `"Map.Toronto"`). |

### Combat

| Command | Parameters | Description |
|---|---|---|
| `select_combat_action` | `actor` (1-based party index), `action` (`Attack`/`Move`/`Flee`/`None`) | Plan an action for a party member before the round starts. |
| `select_combat_target` | `target_tile` (tile index 0–29) | Select the target tile after choosing Attack or Move. |

### Dialogs / events

| Command | Parameters | Description |
|---|---|---|
| `talk_npc` | `npc_id` | Start a dialogue with an NPC. |
| `respond` | `option` (integer) | Select a dialogue option. |
| `dismiss_message` | — | Dismiss the current message/dialog. |
| `raise_event` | `event` (event string) | Fire any game event by its string representation. |

### Economy

| Command | Parameters | Description |
|---|---|---|
| `modify_gold` | `amount` | Add (positive) or remove (negative) gold from the party. |
| `equip_item` | `member`, `item`, `slot` | **Broken stub** — currently ignores `member`/`item`, always equips Tom with the Sword. See Bug #10. |

### Save / load

| Command | Parameters | Description |
|---|---|---|
| `save_game` | `id` (1–999), `name` | Save to slot. |
| `load_game` | `id` | Load from slot. |
| `start_new_game` | — | Start a new game at the default starting position (Toronto). |
| `quit` | — | Exit the game. |

---

## Events Stream

Every connected `/events` client receives:

| `type` field | Payload | Description |
|---|---|---|
| `"command"` | `payload`: raw JSON string | A command received on `/agent`. |
| `"response"` | `payload`: raw JSON string | The response sent back to the agent. |
| `"log"` | `severity`, `message` | A `LogEvent` raised anywhere in the game. |
| `"script_output"` | `file`, `line` | stdout/stderr line from a running Python script. |
| `"script_done"` | `file`, `exit` | Script finished with exit code. |
| `"script_error"` | `file`, `message` | Script failed to start. |

---

## Monitor Dashboard

The browser dashboard at `http://localhost:7399/monitor` provides:

- **Connection indicator** — green dot when `/events` WebSocket is live.
- **Quick buttons** — one-click commands: Ping, New Game, Scene, Party, Combat, Equip Sword, encounter triggers (2×Skrinn, 3×Skrinn, 1×Krondir), Round, Dismiss.
- **Python Scripts panel** — lists `.py` files from `src/Tools/AgentBridge/test/`; click to run.
- **Event log** — collapsible entries for commands, responses, and log messages.
- **Command input** — send arbitrary JSON commands manually.

---

## Agent Mode in Game

When `--agent` is active, `IsAgentModeEvent` returns `true`. This affects:

- **`ApresCombatDialog`** — auto-dismissed immediately (no mouse click needed).
- **`LogicalCombatTile` context menu** — context menu may be bypassed (agent sends `select_combat_action` directly).

---

## Python Script Example

Scripts in `src/Tools/AgentBridge/test/` can control the game:

```python
import asyncio, json, websockets

async def main():
    async with websockets.connect("ws://localhost:7399/agent") as ws:
        # Start a new game
        await ws.send(json.dumps({"cmd": "start_new_game"}))
        print(await ws.recv())

        # Trigger an encounter
        await ws.send(json.dumps({
            "cmd": "raise_event",
            "event": "encounter MonsterGroup.TwoSkrinn1 CombatBackground.Dungeon"
        }))
        print(await ws.recv())

asyncio.run(main())
```

---

## Known Issues / Open Bugs

| Bug | Description |
|---|---|
| #10 | `equip_item` ignores `member` and `item` parameters; always equips Tom with the Sword. |
