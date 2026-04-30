#!/usr/bin/env python3
import json, time, websocket, threading

WS_URL = "ws://localhost:7399/agent"
EVENTS_URL = "ws://localhost:7399/events"

events_ws = None
log_messages = []

def event_listener():
    global events_ws
    try:
        while True:
            data = events_ws.recv()
            evt = json.loads(data)
            msg = evt.get("msg", "")
            if msg.startswith("[") or "Damage" in msg or "ROUND" in msg or "MONSTER" in msg or "FLEE" in msg:
                log_messages.append(msg)
                print(f"  [LOG] {msg}")
    except:
        pass

def send_cmd(ws, cmd):
    ws.send(json.dumps(cmd))
    return json.loads(ws.recv())

def r(resp):
    return resp["result"] if resp.get("ok") else resp

def get_combat():
    return r(send_cmd(ws, {"cmd": "get_combat"}))["tile_map"]

def print_tile_map(label):
    tile_map = get_combat()
    mobs = [e for e in tile_map if e and e.get("type") == "Monster"]
    party = [e for e in tile_map if e and e.get("type") == "Party"]
    print(f"\n=== {label} ===")
    for m in mobs:
        print(f"  Monster: {m['name']} tile={m['tile']} hp={m['hp']}/{m['max_hp']}")
    for p in party:
        print(f"  Party:   {p['name']} tile={p['tile']} hp={p['hp']}/{p['max_hp']}")
    if not mobs:
        print("  (no monsters)")

ws = websocket.create_connection(WS_URL, timeout=10)
events_ws = websocket.create_connection(EVENTS_URL, timeout=10)

t = threading.Thread(target=event_listener, daemon=True)
t.start()

time.sleep(0.5)
send_cmd(ws, {"cmd": "start_new_game"})
time.sleep(2)

send_cmd(ws, {"cmd": "raise_event", "event": "encounter MonsterGroup.TwoSkrinn1"})
time.sleep(2)

print_tile_map("ENCOUNTER START")
print(f"\n  Log messages so far: {len(log_messages)}")

# Simulate 3 rounds
for i in range(3):
    send_cmd(ws, {"cmd": "raise_event", "event": "begin_combat_round"})
    time.sleep(1)
    print_tile_map(f"ROUND {i+1}")

ws.close()
