#!/usr/bin/env python3
"""
Combat Entry Test for uAlbion.
Connects to the AgentBridge WebSocket, starts a new game, triggers a combat
encounter, and verifies the game is in the Combat scene — without fighting.

Usage:
  1. Start uAlbion in agent mode (keep window VISIBLE, don't minimize):
       cd c:/Repository/UAlbion && dotnet run --project src/ualbion -- -d3d --agent --mute
  2. Run this script:
       python test-combat-enter.py
"""

import asyncio
import json
import sys
import websockets

WS_URI = "ws://localhost:7399/agent"
TEST_GROUP = "MonsterGroup.FourWarniak1OneWarniak3"


def result(r: dict):
    return r.get("result", r)


async def cmd(ws, command: dict) -> dict:
    await ws.send(json.dumps(command))
    raw = await asyncio.wait_for(ws.recv(), timeout=15.0)
    resp = json.loads(raw)
    print(f"  << {resp}")
    return resp


async def raise_event(ws, event_str: str) -> dict:
    return await cmd(ws, {"cmd": "raise_event", "event": event_str})


def get_scene_id(r: dict) -> str:
    return result(r).get("scene_id", "")


async def run():
    print(f"Connecting to {WS_URI}...")
    try:
        async with websockets.connect(WS_URI) as ws:
            print("Connected.\n")

            # 1. Ping
            print("=== 1. Ping ===")
            r = await cmd(ws, {"cmd": "ping"})
            if not r.get("ok"):
                print(f"  FAIL: {r}")
                return

            # 2. Start new game
            print("\n=== 2. Start new game ===")
            r = await cmd(ws, {"cmd": "start_new_game"})
            if not r.get("ok"):
                print(f"  FAIL: {r}")
                return
            await asyncio.sleep(3)

            # 3. Verify overworld scene
            print("\n=== 3. Scene (pre-combat) ===")
            r = await cmd(ws, {"cmd": "get_scene"})
            scene = get_scene_id(r)
            print(f"  scene_id = {scene!r}")

            # 4. Party snapshot
            print("\n=== 4. Party ===")
            r = await cmd(ws, {"cmd": "get_party"})
            members = result(r).get("members", [])
            for m in members:
                print(f"  {m.get('name','?')}: HP {m.get('hp','?')}/{m.get('max_hp','?')}, "
                      f"combat_pos={m.get('combat_position','?')}")

            # 5. Trigger encounter
            print(f"\n=== 5. Trigger encounter ({TEST_GROUP}) ===")
            r = await raise_event(ws, f"encounter {TEST_GROUP}")
            await asyncio.sleep(1)

            # 6. Verify combat scene
            print("\n=== 6. Scene (post-encounter) ===")
            r = await cmd(ws, {"cmd": "get_scene"})
            scene = get_scene_id(r)
            print(f"  scene_id = {scene!r}")

            if "Combat" in scene:
                print("\n  PASS: Combat scene entered successfully.")
            else:
                print(f"\n  FAIL: Expected Combat scene, got {scene!r}")

            print("\n=== Test complete (no combat actions taken) ===")

    except ConnectionRefusedError:
        print(f"ERROR: Could not connect to {WS_URI}")
        print("Start uAlbion first:")
        print("  cd c:/Repository/UAlbion && dotnet run --project src/ualbion -- -d3d --agent --mute")
        sys.exit(1)
    except Exception as e:
        print(f"ERROR: {type(e).__name__}: {e}")
        import traceback; traceback.print_exc()
        sys.exit(1)


if __name__ == "__main__":
    asyncio.run(run())
