#!/usr/bin/env python3
"""Full combat flow test: encounter → attack → victory → return to map."""
import json, time, websocket

WS_URL = "ws://localhost:7399/agent"

def send_cmd(ws, cmd):
    ws.send(json.dumps(cmd))
    return json.loads(ws.recv())

def r(resp):
    return resp["result"] if resp.get("ok") else resp

ws = websocket.create_connection(WS_URL, timeout=10)

# Start new game
print("Starting new game...")
send_cmd(ws, {"cmd": "start_new_game"})
time.sleep(2)

# Trigger encounter
print("Triggering encounter...")
send_cmd(ws, {"cmd": "raise_event", "event": "encounter MonsterGroup.TwoSkrinn1"})
time.sleep(2)

# Check combat state
tile_map = r(send_cmd(ws, {"cmd": "get_combat"}))["tile_map"]
mobs = [e for e in tile_map if e and e.get("type") == "Monster"]
print(f"Encounter started: {len(mobs)} monsters")
for m in mobs:
    print(f"  {m['name']} tile={m['tile']} hp={m['hp']}/{m['max_hp']}")

# Plan attacks for Tom against both monsters
for m in mobs:
    print(f"Planning attack on {m['name']} at tile {m['tile']}...")
    send_cmd(ws, {"cmd": "select_combat_action", "actor": 1, "action": "attack"})
    time.sleep(0.3)
    send_cmd(ws, {"cmd": "select_combat_target", "actor": 1, "target_tile": m["tile"]})
    time.sleep(0.3)

# Begin round
print("Beginning combat round...")
send_cmd(ws, {"cmd": "raise_event", "event": "begin_combat_round"})
time.sleep(2)

# Check result
tile_map = r(send_cmd(ws, {"cmd": "get_combat"}))["tile_map"]
mobs = [e for e in tile_map if e and e.get("type") == "Monster"]
party = [e for e in tile_map if e and e.get("type") == "Party"]

if not mobs:
    print("Victory! All monsters defeated.")
    time.sleep(2)  # Let ApresCombatDialog auto-dismiss

    # Check if we're back on the map
    result = r(send_cmd(ws, {"cmd": "get_map_info"}))
    print(f"Map info: {result}")

    # Check player state
    state = r(send_cmd(ws, {"cmd": "get_state"}))
    print(f"Player state: pos={state.get('position')}, hp={state.get('hp')}, xp={state.get('experience')}")
else:
    print(f"Combat still ongoing: {len(mobs)} monsters remaining")
    for m in mobs:
        print(f"  {m['name']} hp={m['hp']}/{m['max_hp']}")

ws.close()
