#!/usr/bin/env python3
import json, time, websocket

WS_URL = "ws://localhost:7399/agent"

ws = websocket.create_connection(WS_URL, timeout=10)
print("Connected!")

def send_cmd(cmd):
    ws.send(json.dumps(cmd))
    time.sleep(0.5)
    resp = json.loads(ws.recv())
    print(f"CMD: {cmd.get('cmd','?')} -> keys: {list(resp.keys())}")
    return resp

r1 = send_cmd({"cmd": "start_new_game"})
print(json.dumps(r1, indent=2)[:300])

time.sleep(1)
r2 = send_cmd({"cmd": "raise_event", "event": "encounter MonsterGroup.TwoSkrinn1"})
print(json.dumps(r2, indent=2)[:300])

time.sleep(1)
r3 = send_cmd({"cmd": "get_combat"})
print(json.dumps(r3, indent=2)[:1000])

ws.close()
