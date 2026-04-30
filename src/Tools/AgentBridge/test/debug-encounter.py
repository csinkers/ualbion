#!/usr/bin/env python3
import json, time, websocket

WS_URL = "ws://localhost:7399/agent"
EVENTS_URL = "ws://localhost:7399/events"

ev_ws = websocket.create_connection(EVENTS_URL, timeout=10)
cmd_ws = websocket.create_connection(WS_URL, timeout=10)

# Event thread to capture all events
import threading
events = []

def event_listener():
    try:
        while True:
            data = ev_ws.recv()
            evt = json.loads(data)
            events.append(evt)
            if "set_scene" in str(evt) or "encounter" in str(evt).lower():
                print(f"  [EVENT] {evt}")
    except:
        pass

t = threading.Thread(target=event_listener, daemon=True)
t.start()

def send_cmd(ws, cmd):
    ws.send(json.dumps(cmd))
    return json.loads(ws.recv())

# Start new game
print("=== start_new_game ===")
r = send_cmd(cmd_ws, {"cmd": "start_new_game"})
print(f"Response: {r}")
time.sleep(2)

print(f"\nEvents captured so far: {len(events)}")
for e in events[-10:]:
    print(f"  {e}")

# Raise encounter
print("\n=== encounter ===")
r = send_cmd(cmd_ws, {"cmd": "raise_event", "event": "encounter MonsterGroup.TwoSkrinn1"})
print(f"Response: {r}")
time.sleep(2)

print(f"\nEvents after encounter: {len(events)}")
for e in events[-15:]:
    print(f"  {e}")

# Get combat state
print("\n=== get_combat ===")
r = send_cmd(cmd_ws, {"cmd": "get_combat"})
print(json.dumps(r, indent=2)[:800])

cmd_ws.close()
ev_ws.close()
