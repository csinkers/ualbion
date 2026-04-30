#!/usr/bin/env python3
import json, time, websocket

WS_URL = "ws://localhost:7399/agent"

ws = websocket.create_connection(WS_URL, timeout=10)

def send_cmd(cmd):
    ws.send(json.dumps(cmd))
    return json.loads(ws.recv())

def r(resp):
    return resp["result"] if resp.get("ok") else resp

def get_combat():
    return r(send_cmd({"cmd": "get_combat"}))["tile_map"]

def print_tile_map(label, tile_map):
    mobs = [e for e in tile_map if e and e.get("type") == "Monster"]
    party = [e for e in tile_map if e and e.get("type") == "Party"]
    print(f"\n=== {label} ===")
    for m in mobs:
        print(f"  Monster: {m['name']} tile={m['tile']} hp={m['hp']}/{m['max_hp']}")
    for p in party:
        print(f"  Party:   {p['name']} tile={p['tile']} hp={p['hp']}/{p['max_hp']}")

send_cmd({"cmd": "start_new_game"})
time.sleep(2)
send_cmd({"cmd": "raise_event", "event": "encounter MonsterGroup.TwoSkrinn1"})
time.sleep(3)

print_tile_map("ENCOUNTER START", get_combat())

# Simulate 5 rounds
for i in range(5):
    send_cmd({"cmd": "raise_event", "event": "begin_combat_round"})
    time.sleep(1)
    tile_map = get_combat()
    print_tile_map(f"ROUND {i+1}", tile_map)
    if not any(e for e in tile_map if e and e.get("type") == "Monster"):
        print(f"  -> All monsters defeated after round {i+1}")
        break

ws.close()
