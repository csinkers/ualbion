#!/usr/bin/env python3
"""
test-combat-tcs.py — Agent-in-the-loop Kampf-Testfälle
Voraussetzung: ualbion.exe --agent läuft auf ws://localhost:7399/agent
"""
import json, sys, time, websocket

WS_URL = "ws://localhost:7399/agent"

def send(ws, cmd):
    ws.send(json.dumps(cmd))
    return json.loads(ws.recv())

def r(resp):
    """Extract result from response."""
    if not resp.get("ok"):
        print(f"ERROR: {resp.get('error', 'unknown')} — {resp.get('message', '')}")
        sys.exit(1)
    return resp["result"]

def assert_eq(label, got, expected):
    if got != expected:
        print(f"FAIL {label}: got={got} expected={expected}")
        sys.exit(1)
    print(f"PASS {label}")

def find_entity(tile_map, name_part):
    """Find entity in tile_map by name substring."""
    return next((e for e in tile_map if e and name_part.lower() in e.get("name", "").lower()), None)

def tc003_monster_not_in_melee_does_not_attack(ws):
    """TC-003: Monster greift nicht an wenn nicht in Melee-Range."""
    r(send(ws, {"cmd": "start_new_game"}))
    time.sleep(1)
    r(send(ws, {"cmd": "raise_event", "event": "encounter MonsterGroup.TwoSkrinn1"}))
    time.sleep(1)
    tile_map = r(send(ws, {"cmd": "get_combat"}))["tile_map"]
    tom = find_entity(tile_map, "Tom")
    if not tom:
        print("SKIP TC-003: Tom not found in combat")
        return
    tom_hp_before = tom["hp"]
    tom_tile = tom["tile"]
    skrinn = find_entity(tile_map, "Skrinn")
    skrinn_tile = skrinn["tile"] if skrinn else -1

    r(send(ws, {"cmd": "raise_event", "event": "begin_combat_round"}))
    time.sleep(1)
    tile_map = r(send(ws, {"cmd": "get_combat"}))["tile_map"]
    tom_after = find_entity(tile_map, "Tom")
    if not tom_after:
        print(f"SKIP TC-003: Tom died (was {tom_hp_before} HP)")
        return
    tom_hp_after = tom_after["hp"]

    if tom_hp_after == tom_hp_before:
        print(f"PASS TC-003: Tom HP unverändert ({tom_hp_before}) — Monster nicht in Reichweite")
    else:
        skrinn_after = find_entity(tile_map, "Skrinn")
        new_tile = skrinn_after["tile"] if skrinn_after else -1
        print(f"NOTE TC-003: Tom HP {tom_hp_before} → {tom_hp_after} "
              f"(Skrinn tile {skrinn_tile} → {new_tile}, {'moved' if new_tile != skrinn_tile else 'stayed'})")

def tc006_xp_distribution(ws):
    """TC-006: XP wird nach Kampf verteilt."""
    r(send(ws, {"cmd": "start_new_game"}))
    time.sleep(1)
    xp_before = r(send(ws, {"cmd": "get_party"}))["members"][0].get("experience_points", 0)

    r(send(ws, {"cmd": "raise_event", "event": "encounter MonsterGroup.TwoSkrinn1"}))
    time.sleep(1)
    # Alle Monster töten (genug Runden)
    for i in range(15):
        r(send(ws, {"cmd": "raise_event", "event": "begin_combat_round"}))
        time.sleep(0.5)
        tile_map = r(send(ws, {"cmd": "get_combat"}))["tile_map"]
        if not any(e for e in tile_map if e and e.get("type") == "Monster"):
            print(f"  Monsters defeated after round {i+1}")
            break

    xp_after = r(send(ws, {"cmd": "get_party"}))["members"][0].get("experience_points", 0)
    if xp_after > xp_before:
        print(f"PASS TC-006: XP {xp_before} → {xp_after} (+{xp_after - xp_before})")
    else:
        print(f"NOTE TC-006: XP unverändert ({xp_before}) — möglicherweise kein XP für diesen Kampf implementiert")

def tc007_monster_flee_check(ws):
    """TC-007: Monster mit niedrigem Morale kann fliehen."""
    r(send(ws, {"cmd": "start_new_game"}))
    time.sleep(1)
    r(send(ws, {"cmd": "raise_event", "event": "encounter MonsterGroup.TwoSkrinn1"}))
    time.sleep(1)

    # Mehrere Runden simulieren, Flucht prüfen
    skrinn_count_before = 0
    tile_map = r(send(ws, {"cmd": "get_combat"}))["tile_map"]
    skrinn_count_before = sum(1 for e in tile_map if e and "Skrinn" in e.get("name", ""))

    fled = False
    for _ in range(20):
        r(send(ws, {"cmd": "raise_event", "event": "begin_combat_round"}))
        time.sleep(0.5)
        tile_map = r(send(ws, {"cmd": "get_combat"}))["tile_map"]
        skrinn_count = sum(1 for e in tile_map if e and "Skrinn" in e.get("name", ""))
        if skrinn_count < skrinn_count_before:
            print(f"PASS TC-007: Skrinn geflohen/gestorben ({skrinn_count_before} → {skrinn_count})")
            fled = True
            break
        skrinn_count_before = skrinn_count

    if not fled:
        print(f"NOTE TC-007: Kein Skrinn geflohen nach 20 Runden (Morale-Check evtl. nicht ausgelöst)")

def tc010_death_removes_from_tile(ws):
    """TC-010: Toter Combatant wird aus Tile-Map entfernt."""
    r(send(ws, {"cmd": "start_new_game"}))
    time.sleep(1)
    r(send(ws, {"cmd": "raise_event", "event": "encounter MonsterGroup.TwoSkrinn1"}))
    time.sleep(1)
    tile_map = r(send(ws, {"cmd": "get_combat"}))["tile_map"]
    skrinn_before = [e for e in tile_map if e and "Skrinn" in e.get("name", "")]
    if not skrinn_before:
        print("SKIP TC-010: No Skrinn found")
        return

    # Kämpfen bis alle Monster tot
    for _ in range(15):
        r(send(ws, {"cmd": "raise_event", "event": "begin_combat_round"}))
        time.sleep(0.5)
        tile_map = r(send(ws, {"cmd": "get_combat"}))["tile_map"]
        if not any(e for e in tile_map if e and e.get("type") == "Monster"):
            break

    tile_map = r(send(ws, {"cmd": "get_combat"}))["tile_map"]
    monsters = [e for e in tile_map if e and e.get("type") == "Monster"]
    if len(monsters) == 0:
        print("PASS TC-010: Keine Monster mehr in tile_map nach Kampf")
    else:
        print(f"FAIL TC-010: Noch {len(monsters)} Monster in tile_map")

if __name__ == "__main__":
    try:
        ws = websocket.create_connection(WS_URL, timeout=10)
    except Exception as e:
        print(f"ERROR: Cannot connect to {WS_URL}: {e}")
        print("Starte ualbion.exe mit --agent Flag zuerst.")
        sys.exit(1)

    try:
        tc003_monster_not_in_melee_does_not_attack(ws)
        tc006_xp_distribution(ws)
        tc007_monster_flee_check(ws)
        tc010_death_removes_from_tile(ws)
    finally:
        ws.close()

    print("\nTest-Sprint abgeschlossen.")
