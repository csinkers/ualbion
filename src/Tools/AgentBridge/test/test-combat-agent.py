#!/usr/bin/env python3
"""
Combat Agent Test Loop for uAlbion.
Connects to the AgentBridge WebSocket (ws://localhost:7399/agent),
starts a new game, triggers a combat encounter, and runs through it.

Usage:
  1. Start uAlbion in agent mode (keep window VISIBLE, don't minimize):
       cd c:/Repository/UAlbion && dotnet run --project src/ualbion -- -d3d --agent --mute
  2. Run this script:
       python test-combat-agent.py

Notes:
  - Group 109 (FourWarniak1OneWarniak3) has a Warniak1 at tile 12 (row2,col0).
    Tom spawns at tile 18 (row3,col0) — Chebyshev distance 1 → adjacent → melee works.
  - Keep the game window VISIBLE. D3D11 pauses rendering when minimized, which blocks commands.
"""

import asyncio
import json
import sys
import websockets

WS_URI = "ws://localhost:7399/agent"

# MonsterGroup 109: FourWarniak1OneWarniak3
# Warniak1 at tile 12 (row2,col0) — adjacent to Tom's default spawn at tile 18 (row3,col0)
TEST_GROUP = "MonsterGroup.FourWarniak1OneWarniak3"
MONSTER_TILE = 12   # Warniak1 front-row position
MAX_ROUNDS = 20


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


def print_party(members):
    for m in members:
        pos = m.get("combat_position", "?")
        print(f"  {m.get('name','?')}: HP {m.get('hp','?')}/{m.get('max_hp','?')}, "
              f"XP {m.get('experience_points','?')}, Lv {m.get('level','?')}, "
              f"combat_pos={pos}")


async def run_combat_test():
    print(f"Connecting to {WS_URI}...")
    try:
        async with websockets.connect(WS_URI) as ws:
            print("Connected.\n")

            # 1. Ping
            print("=== 1. Ping ===")
            await cmd(ws, {"cmd": "ping"})

            # 2. Start new game
            print("\n=== 2. Start new game ===")
            await cmd(ws, {"cmd": "start_new_game"})
            await asyncio.sleep(3)

            # 3. Scene
            print("\n=== 3. Scene ===")
            r = await cmd(ws, {"cmd": "get_scene"})
            print(f"  scene_id = {get_scene_id(r)}")

            # 4. Party pre-combat
            print("\n=== 4. Party (pre-combat) ===")
            r = await cmd(ws, {"cmd": "get_party"})
            members = result(r).get("members", [])
            print_party(members)
            pre_xp = {m["id"]: m.get("experience_points", 0) for m in members}

            # Determine Tom's combat position
            tom = next((m for m in members), None)
            tom_pos = tom.get("combat_position", 18) if tom else 18
            print(f"  Tom combat_pos={tom_pos}, attacking tile={MONSTER_TILE}")

            # 5. Trigger encounter
            print(f"\n=== 5. Encounter ({TEST_GROUP}) ===")
            await raise_event(ws, f"encounter {TEST_GROUP}")
            await asyncio.sleep(1)

            # 6. Verify combat scene
            print("\n=== 6. Scene after encounter ===")
            r = await cmd(ws, {"cmd": "get_scene"})
            scene = get_scene_id(r)
            print(f"  scene_id = {scene}")
            if "Combat" not in scene:
                print("  WARNING: Not in combat scene")
                return

            # 7. Combat round loop
            print(f"\n=== 7. Combat round loop (max {MAX_ROUNDS} rounds) ===")
            for round_num in range(1, MAX_ROUNDS + 1):
                print(f"\n--- Round {round_num} ---")

                # Plan: Tom attacks the front monster tile
                r1 = await raise_event(ws, f"select_combat_action {tom_pos} Attack")
                r2 = await raise_event(ws, f"select_combat_target {MONSTER_TILE}")

                # Execute round
                r = await raise_event(ws, "begin_combat_round")
                if not r.get("ok"):
                    print(f"  begin_combat_round failed: {r}")
                    break

                await asyncio.sleep(2)

                # Check scene
                try:
                    r = await cmd(ws, {"cmd": "get_scene"})
                    scene = get_scene_id(r)
                    print(f"  scene_id = {scene}")
                except asyncio.TimeoutError:
                    print("  TIMEOUT on get_scene — game may be hung (dialog waiting?)")
                    # Try dismiss
                    try:
                        await raise_event(ws, "dismiss_message")
                        await asyncio.sleep(1)
                        r = await cmd(ws, {"cmd": "get_scene"})
                        scene = get_scene_id(r)
                        print(f"  scene_id after dismiss = {scene}")
                    except Exception as e:
                        print(f"  Dismiss failed: {e}")
                        break

                if "Combat" not in scene:
                    print("  Combat ended")
                    break

            # 8. Party post-combat
            print("\n=== 8. Party (post-combat) ===")
            try:
                r = await cmd(ws, {"cmd": "get_party"})
                post_members = result(r).get("members", [])
                print_party(post_members)

                for m in post_members:
                    mid = m["id"]
                    xp_before = pre_xp.get(mid, 0)
                    xp_after = m.get("experience_points", 0)
                    delta = xp_after - xp_before
                    verdict = "✓ XP gained (victory confirmed)" if delta > 0 else "— no XP change (defeat or no kills)"
                    print(f"  {m.get('name','?')}: XP {xp_before} → {xp_after} (+{delta}) {verdict}")
            except asyncio.TimeoutError:
                print("  TIMEOUT: game unresponsive after combat")

            print("\n=== Test complete ===")

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
    asyncio.run(run_combat_test())
