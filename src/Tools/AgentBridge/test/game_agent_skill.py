#!/usr/bin/env python3
"""
uAlbion Game Agent Skill — Universelle CLI für KI-gesteuerte Spieltests.

Verwendung (für andere AIs / OpenCode / Gemini / ModelRelay):
  python game_agent_skill.py start              # Spiel starten (Hintergrund)
  python game_agent_skill.py status             # Verbindungsstatus prüfen
  python game_agent_skill.py cmd <json>         # Beliebigen WS-Befehl senden
  python game_agent_skill.py event <event_str>  # raise_event senden
  python game_agent_skill.py combat [--rounds N] [--group GROUP] [--llm]
                                                # Kampf-Loop ausführen
  python game_agent_skill.py new_game           # Neues Spiel starten + warten
  python game_agent_skill.py get_scene          # Aktuellen Szenen-Zustand holen
  python game_agent_skill.py get_party          # Party-Status holen
  python game_agent_skill.py get_combat         # Kampf-Status holen
  python game_agent_skill.py equip <member> <item> [<slot>]  # Item ausrüsten

Output ist immer JSON ({"ok": bool, "result": ...}) — maschinenlesbar.
Fehler gehen auf stderr; JSON-Ergebnis immer auf stdout.

WebSocket: ws://localhost:7399/agent
Spiel starten: cd c:/Repository/UAlbion && dotnet run --project src/ualbion -- -d3d --agent --mute
"""

import asyncio
import json
import os
import subprocess
import sys
import time
from typing import Optional

import websockets

WS_URI = "ws://localhost:7399/agent"
CONNECT_TIMEOUT = 5.0
CMD_TIMEOUT = 15.0

# ─── Niedriglevel WebSocket-Helfer ───────────────────────────────────────────

async def ws_cmd(ws, command: dict) -> dict:
    await ws.send(json.dumps(command))
    raw = await asyncio.wait_for(ws.recv(), timeout=CMD_TIMEOUT)
    return json.loads(raw)


async def ws_event(ws, event_str: str) -> dict:
    return await ws_cmd(ws, {"cmd": "raise_event", "event": event_str})


async def connect() -> Optional[websockets.WebSocketClientProtocol]:
    try:
        ws = await asyncio.wait_for(
            websockets.connect(WS_URI),
            timeout=CONNECT_TIMEOUT
        )
        return ws
    except (ConnectionRefusedError, OSError, asyncio.TimeoutError):
        return None


# ─── Sub-Kommandos ────────────────────────────────────────────────────────────

async def do_status() -> dict:
    ws = await connect()
    if ws is None:
        return {"ok": False, "error": "game not running or agent not connected", "ws_uri": WS_URI}
    async with ws:
        r = await ws_cmd(ws, {"cmd": "ping"})
        scene = await ws_cmd(ws, {"cmd": "get_scene"})
    return {"ok": True, "ping": r, "scene": scene.get("result", scene)}


async def do_cmd(command: dict) -> dict:
    ws = await connect()
    if ws is None:
        return {"ok": False, "error": "game not running"}
    async with ws:
        return await ws_cmd(ws, command)


async def do_event(event_str: str) -> dict:
    return await do_cmd({"cmd": "raise_event", "event": event_str})


async def do_new_game(wait_secs: float = 3.0) -> dict:
    ws = await connect()
    if ws is None:
        return {"ok": False, "error": "game not running"}
    async with ws:
        r = await ws_cmd(ws, {"cmd": "start_new_game"})
        await asyncio.sleep(wait_secs)
        scene = await ws_cmd(ws, {"cmd": "get_scene"})
    return {"ok": r.get("ok", False), "start_new_game": r, "scene": scene.get("result", scene)}


async def do_equip(member: str, item: str, slot: str = "RightHand") -> dict:
    return await do_cmd({"cmd": "equip_item", "member": member, "item": item, "slot": slot})


async def do_combat_loop(
    group: str = "MonsterGroup.FourWarniak1OneWarniak3",
    monster_tile: int = 12,
    max_rounds: int = 20,
    llm_mode: bool = False,
) -> dict:
    """
    Vollständiger Kampf-Loop.
    llm_mode=True: Nach jeder Runde wird der Zustand als JSON ausgegeben und
    auf stdin gewartet (JSON {"action": "attack"|"move"|"skip", "target": int, "from": int}).
    Damit kann eine externe KI (Gemini, ModelRelay) Entscheidungen treffen.
    """
    ws = await connect()
    if ws is None:
        return {"ok": False, "error": "game not running"}

    results = {"rounds": [], "verdict": "incomplete"}

    async with ws:
        # Party-Info holen
        party_r = await ws_cmd(ws, {"cmd": "get_party"})
        members = party_r.get("result", {}).get("members", party_r.get("members", []))
        tom = next((m for m in members), None)
        tom_pos = tom.get("combat_position", 18) if tom else 18
        pre_xp = {m["id"]: m.get("experience_points", 0) for m in members}

        # Encounter starten
        enc = await ws_event(ws, f"encounter {group}")
        await asyncio.sleep(1)

        scene_r = await ws_cmd(ws, {"cmd": "get_scene"})
        scene_id = scene_r.get("result", {}).get("scene_id", "")
        if "Combat" not in scene_id:
            return {"ok": False, "error": f"combat not entered, scene={scene_id}", "encounter": enc}

        for round_num in range(1, max_rounds + 1):
            round_info = {"round": round_num}

            if llm_mode:
                # Zustand ausgeben; auf KI-Entscheidung warten
                state = {
                    "prompt": "decide_action",
                    "round": round_num,
                    "tom_pos": tom_pos,
                    "monster_tile": monster_tile,
                    "scene_id": scene_id,
                }
                print(json.dumps(state), flush=True)
                try:
                    line = sys.stdin.readline().strip()
                    decision = json.loads(line) if line else {"action": "attack", "target": monster_tile}
                except (json.JSONDecodeError, EOFError):
                    decision = {"action": "attack", "target": monster_tile}

                action = decision.get("action", "attack")
                target = decision.get("target", monster_tile)
                actor_pos = decision.get("from", tom_pos)
            else:
                action = "Attack"
                target = monster_tile
                actor_pos = tom_pos

            # Aktion auswählen
            r1 = await ws_event(ws, f"select_combat_action {actor_pos} {action.capitalize()}")
            r2 = await ws_event(ws, f"select_combat_target {target}")
            r3 = await ws_event(ws, "begin_combat_round")
            round_info["select_action"] = r1.get("result", r1)
            round_info["select_target"] = r2.get("result", r2)
            round_info["begin_round"] = r3.get("result", r3)

            await asyncio.sleep(2)

            # Szene nach Runde prüfen
            try:
                scene_r = await asyncio.wait_for(
                    ws_cmd(ws, {"cmd": "get_scene"}), timeout=10.0
                )
                scene_id = scene_r.get("result", {}).get("scene_id", "")
            except asyncio.TimeoutError:
                # Dialog blockiert — dismiss versuchen
                try:
                    await ws_event(ws, "dismiss_message")
                    await asyncio.sleep(1)
                    scene_r = await ws_cmd(ws, {"cmd": "get_scene"})
                    scene_id = scene_r.get("result", {}).get("scene_id", "")
                    round_info["dismissed_dialog"] = True
                except Exception as e:
                    round_info["hang_error"] = str(e)
                    results["rounds"].append(round_info)
                    results["verdict"] = "hung"
                    return {"ok": False, "rounds": results}

            round_info["scene_id"] = scene_id
            results["rounds"].append(round_info)

            if "Combat" not in scene_id:
                results["verdict"] = "combat_ended"
                break
        else:
            results["verdict"] = "max_rounds_reached"

        # Post-Combat Party
        try:
            post_r = await asyncio.wait_for(
                ws_cmd(ws, {"cmd": "get_party"}), timeout=10.0
            )
            post_members = post_r.get("result", {}).get("members", post_r.get("members", []))
            xp_deltas = {}
            for m in post_members:
                mid = m["id"]
                delta = m.get("experience_points", 0) - pre_xp.get(mid, 0)
                xp_deltas[m.get("name", mid)] = {"before": pre_xp.get(mid, 0),
                                                  "after": m.get("experience_points", 0),
                                                  "delta": delta}
            results["xp_deltas"] = xp_deltas
            results["victory"] = any(v["delta"] > 0 for v in xp_deltas.values())
        except asyncio.TimeoutError:
            results["post_combat_timeout"] = True

    return {"ok": True, "group": group, "tom_pos": tom_pos, **results}


async def start_game_async(timeout: int = 120) -> dict:
    """
    Spiel starten und async warten bis WebSocket-Port 7399 offen ist.
    dotnet run kompiliert zuerst (~15–40s), danach startet das Spiel (~5s).
    timeout: maximale Wartezeit in Sekunden (default 120).
    """
    import socket

    # Bereits laufend?
    try:
        s = socket.create_connection(("localhost", 7399), timeout=1)
        s.close()
        return {"ok": True, "already_running": True}
    except OSError:
        pass

    print(f"Starting uAlbion (dotnet run, waiting up to {timeout}s for port 7399)...",
          file=sys.stderr, flush=True)

    dotnet_cmd = [
        "dotnet", "run", "--project", "src/ualbion",
        "--", "-d3d", "--agent", "--mute"
    ]
    proc = subprocess.Popen(
        dotnet_cmd,
        cwd="c:/Repository/UAlbion",
        stdout=subprocess.PIPE,   # nicht DEVNULL — so sehen wir Kompilierfehler
        stderr=subprocess.STDOUT,
        creationflags=subprocess.CREATE_NEW_PROCESS_GROUP if sys.platform == "win32" else 0
    )

    # Async auf Port warten — alle 2s prüfen, stdout leeren damit der Puffer nicht voll läuft
    async def drain_stdout():
        loop = asyncio.get_event_loop()
        while proc.poll() is None:
            try:
                line = await asyncio.wait_for(
                    loop.run_in_executor(None, proc.stdout.readline), timeout=2.0
                )
                if line:
                    print(f"  [game] {line.decode(errors='replace').rstrip()}", file=sys.stderr)
            except asyncio.TimeoutError:
                pass

    async def wait_for_port():
        for elapsed in range(0, timeout, 2):
            await asyncio.sleep(2)
            if proc.poll() is not None:
                return {"ok": False, "error": "game process exited during startup", "pid": proc.pid}
            try:
                s = socket.create_connection(("localhost", 7399), timeout=1)
                s.close()
                print(f"  Port 7399 open after ~{elapsed + 2}s", file=sys.stderr, flush=True)
                return {"ok": True, "pid": proc.pid, "started": True, "elapsed_s": elapsed + 2}
            except OSError:
                pass
        return {"ok": False, "error": f"timeout after {timeout}s waiting for port 7399", "pid": proc.pid}

    # Stdout-Drain und Port-Warten parallel
    drain_task = asyncio.create_task(drain_stdout())
    result = await wait_for_port()
    drain_task.cancel()
    try:
        await drain_task
    except asyncio.CancelledError:
        pass
    return result


def start_game(timeout: int = 120) -> dict:
    """Synchroner Wrapper um start_game_async (für CLI-Nutzung)."""
    return asyncio.run(start_game_async(timeout))


# ─── Einstiegspunkt ───────────────────────────────────────────────────────────

def out(data: dict):
    print(json.dumps(data, ensure_ascii=False, indent=2))


def main():
    args = sys.argv[1:]
    if not args:
        print(__doc__, file=sys.stderr)
        sys.exit(1)

    sub = args[0].lower()

    if sub == "start":
        out(start_game())

    elif sub == "status":
        out(asyncio.run(do_status()))

    elif sub == "cmd":
        if len(args) < 2:
            out({"ok": False, "error": "usage: cmd <json_object>"})
            sys.exit(1)
        try:
            command = json.loads(args[1])
        except json.JSONDecodeError as e:
            out({"ok": False, "error": f"invalid JSON: {e}"})
            sys.exit(1)
        out(asyncio.run(do_cmd(command)))

    elif sub == "event":
        if len(args) < 2:
            out({"ok": False, "error": "usage: event <event_string>"})
            sys.exit(1)
        out(asyncio.run(do_event(" ".join(args[1:]))))

    elif sub in ("new_game", "newgame"):
        wait = float(args[1]) if len(args) > 1 else 3.0
        out(asyncio.run(do_new_game(wait)))

    elif sub == "get_scene":
        out(asyncio.run(do_cmd({"cmd": "get_scene"})))

    elif sub == "get_party":
        out(asyncio.run(do_cmd({"cmd": "get_party"})))

    elif sub == "get_combat":
        out(asyncio.run(do_cmd({"cmd": "get_combat"})))

    elif sub == "equip":
        # equip PartyMember.Tom Item.Sword [RightHand]
        if len(args) < 3:
            out({"ok": False, "error": "usage: equip <member> <item> [<slot>]"})
            sys.exit(1)
        slot = args[3] if len(args) > 3 else "RightHand"
        out(asyncio.run(do_equip(args[1], args[2], slot)))

    elif sub == "combat":
        import argparse
        p = argparse.ArgumentParser(prog="game_agent_skill.py combat")
        p.add_argument("--rounds", type=int, default=20)
        p.add_argument("--group", default="MonsterGroup.FourWarniak1OneWarniak3")
        p.add_argument("--tile", type=int, default=12)
        p.add_argument("--llm", action="store_true",
                       help="LLM-Loop: gibt Zustand als JSON aus, wartet auf Entscheidung von stdin")
        opts = p.parse_args(args[1:])
        out(asyncio.run(do_combat_loop(
            group=opts.group,
            monster_tile=opts.tile,
            max_rounds=opts.rounds,
            llm_mode=opts.llm,
        )))

    else:
        out({"ok": False, "error": f"unknown subcommand: {sub!r}", "valid": [
            "start", "status", "cmd", "event", "new_game",
            "get_scene", "get_party", "get_combat", "equip", "combat"
        ]})
        sys.exit(1)


if __name__ == "__main__":
    main()
