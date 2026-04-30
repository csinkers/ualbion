#!/usr/bin/env python3
"""Debug combat action planning."""
import json, time, websocket

WS_URL = "ws://localhost:7399/agent"

def send_cmd(ws, cmd):
    ws.send(json.dumps(cmd))
    return json.loads(ws.recv())

def r(resp):
    return resp["result"] if resp.get("ok") else resp

ws = websocket.create_connection(WS_URL, timeout=10)

print("Starting new game...")
send_cmd(ws, {"cmd": "start_new_game"})
time.sleep(2)

print("Triggering encounter...")
send_cmd(ws, {"cmd": "raise_event", "event": "encounter MonsterGroup.TwoSkrinn1"})
time.sleep(2)

tile_map = r(send_cmd(ws, {"cmd": "get_combat"}))["tile_map"]
mobs = [e for e in tile_map if e and e.get("type") == "Monster"]
print(f"Monsters: {[(m['name'], m['tile']) for m in mobs]}")

# Plan attack
print("\nPlanning attack on monster at tile", mobs[0]["tile"])
resp1 = send_cmd(ws, {"cmd": "select_combat_action", "actor": 1, "action": "attack"})
print(f"  select_combat_action: {resp1}")
time.sleep(0.3)

resp2 = send_cmd(ws, {"cmd": "select_combat_target", "actor": 1, "target_tile": mobs[0]["tile"]})
print(f"  select_combat_target: {resp2}")
time.sleep(0.3)

# Check planned actions
planned = r(send_cmd(ws, {"cmd": "get_combat"}))
print(f"\nPlanned actions state: {json.dumps(planned, indent=2)}")

print("\nBeginning combat round...")
send_cmd(ws, {"cmd": "raise_event", "event": "begin_combat_round"})
time.sleep(2)

tile_map = r(send_cmd(ws, {"cmd": "get_combat"}))["tile_map"]
mobs_after = [e for e in tile_map if e and e.get("type") == "Monster"]
party_after = [e for e in tile_map if e and e.get("type") == "Party"]
print(f"\nAfter round:")
for m in mobs_after:
    print(f"  Monster: {m['name']} hp={m['hp']}/{m['max_hp']}")
for p in party_after:
    print(f"  Party: {p['name']} hp={p['hp']}/{p['max_hp']}")

ws.close()
