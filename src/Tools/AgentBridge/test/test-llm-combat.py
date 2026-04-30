#!/usr/bin/env python3
"""
LLM-gesteuerte Kampftest-Schleife für uAlbion.
ModelRelay (Tier 2) entscheidet nach jeder Runde die nächste Aktion.

Usage:
  python test-llm-combat.py [--local]   # --local = FastFlowLM/NPU statt ModelRelay
"""

import asyncio
import json
import os
import subprocess
import sys
import argparse
import socket
import websockets

WS_URI = "ws://localhost:7399/agent"
MAX_ROUNDS = 20
TEST_GROUP = "MonsterGroup.FourWarniak1OneWarniak3"
MONSTER_TILE = 12

MODELRELAY_BASE = "http://127.0.0.1:7352/v1"
MODELRELAY_MODEL = "auto-fastest"
LOCAL_BASE = "http://127.0.0.1:11434/v1"
LOCAL_MODEL = "qwen3-it:4b"

SYSTEM_PROMPT = """You are a combat AI agent for uAlbion (a 1995 RPG engine).
Your job: analyze combat state and decide the next action.

Rules:
- Tom is our fighter. He starts at combat tile 18 (row 3, col 0).
- The primary enemy (Warniak1) is usually at tile 12 (row 2, col 0).
- Chebyshev distance <= 1 means melee range.
- Available actions: Attack (if in range), Move (if not in range), Skip.

Respond ONLY with a JSON object, no explanation:
{"action": "Attack"|"Move"|"Skip", "from": <tom_tile_int>, "target": <target_tile_int>, "reason": "<short>"}
"""


def make_client(local: bool):
    from openai import OpenAI
    if local:
        return OpenAI(base_url=LOCAL_BASE, api_key="local"), LOCAL_MODEL
    return OpenAI(base_url=MODELRELAY_BASE, api_key="dummy-key"), MODELRELAY_MODEL


def llm_decide(client, model: str, state: dict) -> dict:
    user_msg = f"Combat state:\n{json.dumps(state, indent=2)}\n\nDecide the next action."
    try:
        resp = client.chat.completions.create(
            model=model,
            messages=[
                {"role": "system", "content": SYSTEM_PROMPT},
                {"role": "user", "content": user_msg},
            ],
            max_tokens=200,
            temperature=0.1,
        )
        content = resp.choices[0].message.content.strip()
        # JSON aus Antwort extrahieren (falls von Markdown umgeben)
        if "```" in content:
            content = content.split("```")[1]
            if content.startswith("json"):
                content = content[4:]
        return json.loads(content.strip())
    except Exception as e:
        print(f"  [LLM error] {e} — fallback: Attack {MONSTER_TILE}", file=sys.stderr)
        return {"action": "Attack", "from": 18, "target": MONSTER_TILE, "reason": "fallback"}


async def ws_cmd(ws, command: dict) -> dict:
    await ws.send(json.dumps(command))
    raw = await asyncio.wait_for(ws.recv(), timeout=15.0)
    return json.loads(raw)


async def ws_event(ws, event_str: str) -> dict:
    return await ws_cmd(ws, {"cmd": "raise_event", "event": event_str})


async def ensure_game_running(timeout: int = 120) -> dict:
    """
    Spiel starten falls nicht aktiv. Startet dotnet run im Hintergrund,
    liest stdout parallel (verhindert Puffer-Blockade), wartet auf Port 7399.
    """
    try:
        s = socket.create_connection(("localhost", 7399), timeout=1)
        s.close()
        print("Game already running (port 7399 open).", flush=True)
        return {"ok": True, "already_running": True}
    except OSError:
        pass

    print(f"Starting uAlbion (dotnet run) — waiting up to {timeout}s for port 7399...", flush=True)
    proc = subprocess.Popen(
        ["dotnet", "run", "--project", "src/ualbion", "--", "-d3d", "--agent", "--mute"],
        cwd="c:/Repository/UAlbion",
        stdout=subprocess.PIPE,
        stderr=subprocess.STDOUT,
        creationflags=subprocess.CREATE_NEW_PROCESS_GROUP if sys.platform == "win32" else 0,
    )

    loop = asyncio.get_event_loop()

    async def drain():
        while proc.poll() is None:
            try:
                line = await asyncio.wait_for(
                    loop.run_in_executor(None, proc.stdout.readline), timeout=2.0
                )
                if line:
                    print(f"  [game] {line.decode(errors='replace').rstrip()}", flush=True)
            except asyncio.TimeoutError:
                pass

    drain_task = asyncio.create_task(drain())

    for elapsed in range(2, timeout + 1, 2):
        await asyncio.sleep(2)
        if proc.poll() is not None:
            drain_task.cancel()
            return {"ok": False, "error": "game process exited unexpectedly", "pid": proc.pid}
        try:
            s = socket.create_connection(("localhost", 7399), timeout=1)
            s.close()
            print(f"  Port 7399 open after ~{elapsed}s.", flush=True)
            drain_task.cancel()
            return {"ok": True, "pid": proc.pid, "started": True, "elapsed_s": elapsed}
        except OSError:
            pass

    drain_task.cancel()
    return {"ok": False, "error": f"timeout after {timeout}s", "pid": proc.pid}


async def run(local: bool):
    client, model = make_client(local)
    print(f"Using {'FastFlowLM/NPU (local)' if local else 'ModelRelay (Tier 2)'} — model: {model}")

    # Spiel starten falls nötig
    startup = await ensure_game_running()
    if not startup["ok"]:
        print(f"ERROR: {startup}", file=sys.stderr)
        sys.exit(1)

    try:
        async with websockets.connect(WS_URI) as ws:
            print(f"Connected to {WS_URI}\n")

            # Ping
            await ws_cmd(ws, {"cmd": "ping"})

            # Neues Spiel
            print("=== Starting new game ===")
            await ws_cmd(ws, {"cmd": "start_new_game"})
            await asyncio.sleep(3)

            # Sword equippen
            print("=== Equipping sword ===")
            eq = await ws_cmd(ws, {"cmd": "equip_item", "member": "PartyMember.Tom",
                                   "item": "Item.Sword", "slot": "RightHand"})
            print(f"  equip: {eq}")

            # Party-Snapshot
            party_r = await ws_cmd(ws, {"cmd": "get_party"})
            members = party_r.get("result", {}).get("members", party_r.get("members", []))
            tom = next((m for m in members), None)
            tom_pos = tom.get("combat_position", 18) if tom else 18
            pre_xp = {m["id"]: m.get("experience_points", 0) for m in members}
            print(f"  Tom at tile {tom_pos}, pre-XP: {pre_xp}")

            # Encounter
            print(f"\n=== Encounter: {TEST_GROUP} ===")
            await ws_event(ws, f"encounter {TEST_GROUP}")
            await asyncio.sleep(1)

            scene_r = await ws_cmd(ws, {"cmd": "get_scene"})
            scene_id = scene_r.get("result", {}).get("scene_id", "")
            if "Combat" not in scene_id:
                print(f"FAIL: not in combat — scene={scene_id}")
                return

            print(f"  In combat! scene={scene_id}\n")

            # Kampf-Loop
            for rnd in range(1, MAX_ROUNDS + 1):
                print(f"--- Round {rnd} ---")

                # Kampf-Zustand für LLM holen
                combat_r = await ws_cmd(ws, {"cmd": "get_combat"})
                state = {
                    "round": rnd,
                    "tom_pos": tom_pos,
                    "monster_tile": MONSTER_TILE,
                    "scene_id": scene_id,
                    "combat": combat_r.get("result", combat_r),
                }

                # LLM entscheidet
                decision = llm_decide(client, model, state)
                print(f"  LLM decision: {decision}")

                action = decision.get("action", "Attack")
                from_tile = decision.get("from", tom_pos)
                target = decision.get("target", MONSTER_TILE)

                # Aktion ausführen
                r1 = await ws_event(ws, f"select_combat_action {from_tile} {action}")
                r2 = await ws_event(ws, f"select_combat_target {target}")
                r3 = await ws_event(ws, "begin_combat_round")
                print(f"  select_action: {r1.get('result', r1)}")
                print(f"  begin_round:   {r3.get('result', r3)}")

                await asyncio.sleep(2)

                # Szene prüfen
                try:
                    scene_r = await asyncio.wait_for(
                        ws_cmd(ws, {"cmd": "get_scene"}), timeout=10.0
                    )
                    scene_id = scene_r.get("result", {}).get("scene_id", "")
                except asyncio.TimeoutError:
                    print("  TIMEOUT — trying dismiss_message")
                    try:
                        await ws_event(ws, "dismiss_message")
                        await asyncio.sleep(1)
                        scene_r = await ws_cmd(ws, {"cmd": "get_scene"})
                        scene_id = scene_r.get("result", {}).get("scene_id", "")
                    except Exception as e:
                        print(f"  HUNG: {e}")
                        return

                print(f"  scene: {scene_id}")
                if "Combat" not in scene_id:
                    print("\n=== Combat ended ===")
                    break
            else:
                print(f"\n=== Max rounds ({MAX_ROUNDS}) reached ===")

            # Post-Combat
            post_r = await ws_cmd(ws, {"cmd": "get_party"})
            post_members = post_r.get("result", {}).get("members", post_r.get("members", []))
            print("\n=== Results ===")
            for m in post_members:
                before = pre_xp.get(m["id"], 0)
                after = m.get("experience_points", 0)
                delta = after - before
                verdict = "VICTORY" if delta > 0 else "no XP gain"
                print(f"  {m.get('name','?')}: XP {before} → {after} (+{delta}) [{verdict}]")

    except ConnectionRefusedError:
        print(f"ERROR: Cannot connect to {WS_URI}")
        print("Start uAlbion: cd c:/Repository/UAlbion && dotnet run --project src/ualbion -- -d3d --agent --mute")
        sys.exit(1)


def main():
    p = argparse.ArgumentParser()
    p.add_argument("--local", action="store_true", help="Use FastFlowLM/NPU instead of ModelRelay")
    args = p.parse_args()
    asyncio.run(run(args.local))


if __name__ == "__main__":
    main()
