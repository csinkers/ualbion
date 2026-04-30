# Combat System

Implementation of the turn-based combat system for uAlbion.
Primary reference: Horneman MS-DOS source (`c:/Repository/HornemanAlbion/MS-DOS/221195/ALBION/SRC/COMBAT/`).

---

## Architecture

```
CombatManager          — wires EncounterEvent → pushes Combat scene, owns battle lifecycle
└── Battle             — logical state: mobs, planned actions, round execution
    ├── Monster (×N)   — per-monster state + sprite, handles CombatAnimationEvent
    └── Sprite         — combat background (full-screen)

DialogManager
└── CombatDialog       — combat grid UI (LogicalCombatTile grid + Start Round button)
    └── ApresCombatDialog  — victory screen (XP / gold / food / items)

DamageCalculator       — stateless, pure formula class
```

### Scene flow

```
EncounterEvent
  → PushSceneEvent(Combat)
  → Battle created, added to scene
  → CombatDialogEvent → CombatDialog created in DialogManager
  → player clicks "Start Round" in CombatDialog
      → BeginCombatRoundEvent → Battle.BeginRoundAsync
          → round executes (sorted by Speed)
          → CombatUpdateEvent(10 ticks) — animation wait
          → victory/defeat check
              Victory → ApresCombatDialog → await player dismiss
                       → EndCombatEvent → CombatDialog.Remove()
                       → Complete?.Invoke(Victory)
  → CombatManager.Complete handler
      → scene.Remove(battle)
      → PopSceneEvent → map scene restored
```

---

## Combat Grid

The field is `CombatRows × CombatColumns` tiles (constants from `SavedGame`).
Rows 0..`CombatRowsForMobs-1` are the monster zone; the remaining rows are the party zone.

```
row 0  [enemy front]
row 1  [enemy]
row 2  [enemy back]
row 3  [party front]
row 4  [party back]
```

Tile index = `row * CombatColumns + col`.

---

## Turn Order

Source: Horneman `COMBAT.C Create_sorted_combat_event_list` (Shellsort by Speed).

Each alive combatant acts once per round, sorted descending by `Effective.Attributes.Speed.Current`.
Dead combatants are skipped. Each actor has `Effective.Combat.ActionPoints` actions per round.

---

## Damage Formula

Source: `COMACTS.C Calculate_afflicted_damage`, `Get_rnd_50_100`, `Probe_skill`.

```
rawDamage     = attacker.BaseAttack + attacker.BonusAttack + attacker.DisplayDamage
                + attacker.Strength / 25
rawProtection = defender.BaseDefense

rolledDamage     = rawDamage / 2     + rand(0 .. rawDamage / 2)     // Get_rnd_50_100
rolledProtection = rawProtection / 2 + rand(0 .. rawProtection / 2)

afflicted = max(rolledDamage - rolledProtection, 0)

critical hit (party attacker vs non-party defender):
  if rand(0..99) < attacker.CriticalChance → afflicted = defender.CurrentHP
```

See `DamageCalculator.cs` for implementation.

**DEVIATION:** `END_MONSTER` (boss immunity to crits) flag not yet exposed in `ICharacterSheet` — approximated by `CharacterType.Monster` check.

---

## Monster AI

Source: Horneman `MONLOGIC.C Default_decider`.

Each monster round:
1. **Flee check** — if `(globalDanger + localDanger) / 2 >= Morale` and `Morale != 0`, flee.
   - `globalDanger` = percentage of monsters already dead.
   - `localDanger`  = percentage of own HP lost.
   - `Morale = 0` means "fights to the death".
2. **Target selection** — nearest living party member by column distance.
3. **Attack or Move** — attack if in melee range (Chebyshev ≤ 1), otherwise step toward target.

**DEVIATION:** Adjacency mask from `COMACTS.C Get_close_range_targets` not available in Horneman source snapshot; Chebyshev distance ≤ 1 used as approximation.

---

## Party Actions

Players pre-plan actions before each round via `CombatDialog` (or via the Agent API).

| Action | Source | Notes |
|---|---|---|
| `Attack` | `CLOSE_RANGE_COMACT` / `LONG_RANGE_COMACT` | Melee: Chebyshev ≤ 1. Ranged: any tile if weapon is `LongRangeWeapon`. |
| `Move`   | `MOVE_COMACT` | Exactly one tile (Chebyshev = 1), must be empty. |
| `Flee`   | `FLEE_COMACT` | Only from last party row (row 4). Removes actor from combat. |
| `None`   | `NO_COMACT`   | Skip turn. |
| `CastSpell` / `UseMagicItem` | `CAST_SPELL_COMACT` | **Stub** — not yet implemented (Bug #21). |

Action planning is two-phase:
1. `SelectCombatActionEvent` — sets action type; if Attack/Move enters `SelectingTarget` state.
2. `SelectCombatTargetEvent` — sets target tile, stores `PlannedCombatAction`.

---

## Monster Sprites

Source: `Monster.cs`, `MonsterSprite.cs`.

`Monster` receives `CombatAnimationEvent` and maps `CombatAnimationType` → `CombatAnimationId`:

| Type | AnimationId |
|---|---|
| Attack | Melee |
| Hit | Hit |
| Death | Die |
| Move | Move |
| Flee | Retreat |
| Cast | Magic |

Animations run at ~8 FPS (`FrameDuration = 0.12 s`). After the Die animation completes (or if no frames exist), the sprite is hidden (`IsActive = false`).

Position is mapped from tile index to 3-D world coordinates using fixed field dimensions:
- `fieldWidth = 300`, `fieldDepth = 240`, `fieldDepthOffset = 30`.

---

## Victory / Defeat

After the round's `CombatUpdateEvent` wait:

- **Victory** (`!anyMonsters`): distribute XP, show `ApresCombatDialog`, then fire `EndCombatEvent(Victory)` + `Complete.Invoke`.
- **Party killed** (`!anyParty`): fire `EndCombatEvent(PartyKilled)` + `Complete.Invoke`.
- **Stalemate / retreat**: not yet handled; round continues.

XP is split equally among living party members (source: Horneman `COMBAT.C Added_experience_points / Enter_Apres_combat`).

Loot (gold, food, items) is collected from dead monster inventories during `ApplyDamageAndCleanup`.

---

## Known Deviations from Original

| # | Area | Deviation |
|---|---|---|
| D1 | Monster stat randomisation | `Clone_monster_data` ±5% not implemented (read-only `IEffectiveCharacterSheet`). |
| D2 | Boss crit immunity | `END_MONSTER` flag approximated by `CharacterType.Monster`. |
| D3 | Adjacency mask | Chebyshev ≤ 1 replaces exact `Get_close_range_targets` bitmask. |
| D4 | Spell/magic items | Stub — `SelectSpellDialog` not yet implemented (Bug #21). |
| D5 | Apres-combat text | SystemText IDs for victory message not confirmed in SR output; hardcoded English strings used. |
| D6 | Game-over screen | No dedicated screen; falls back to main menu on party death. |
