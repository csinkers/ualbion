using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using UAlbion.Api.Eventing;
using UAlbion.Api.Visual;
using UAlbion.Config;
using UAlbion.Core.Visual;
using UAlbion.Formats.Assets.Inv;
using UAlbion.Formats.Assets.Sheets;
using UAlbion.Formats.Assets.Save;
using UAlbion.Formats.Ids;
using UAlbion.Game.Gui.Combat;
using UAlbion.Game.Gui.Dialogs;
using UAlbion.Game.State;
using UAlbion.Game.Events;

namespace UAlbion.Game.Combat;

/// <summary>
/// Contains the logical state of a battle.
/// The top-level combat UI is handled by <see cref="CombatDialog"/>
/// </summary>
public class Battle : GameComponent, IReadOnlyBattle
{
    readonly MonsterGroupId _groupId;
    readonly List<ICombatParticipant> _mobs = [];
    readonly List<ICombatParticipant> _corpses = [];
    readonly ICombatParticipant[] _tiles = new ICombatParticipant[SavedGame.CombatRows * SavedGame.CombatColumns];
    readonly Dictionary<int, PlannedCombatAction> _plannedActions = new();
    readonly List<(ItemId Item, int Amount)> _apresItems = [];
    int _addedExperiencePoints;
    int _apresGold;
    int _apresFood;
    CombatActionType _pendingActionType;

    public IReadOnlyList<ICombatParticipant> Mobs { get; }
    public CombatPlanningState PlanningState { get; private set; } = CombatPlanningState.Planning;
    public int PendingActorPosition { get; private set; } = -1;
    public PlannedCombatAction GetPlannedAction(int tileIndex)
        => _plannedActions.TryGetValue(tileIndex, out var a) ? a : null;

    // Apres combat data (set after battle ends)
    public int XpShare { get; private set; }
    public int ApresGold => _apresGold;
    public int ApresFood => _apresFood;
    public IReadOnlyList<(ItemId Item, int Amount)> ApresItems => _apresItems;

    public event Action<CombatResult> Complete;

    public Battle(MonsterGroupId groupId, SpriteId backgroundId)
    {
        // Fires when an external event source (e.g. a map script) ends combat early.
        // When Battle ends combat internally, it calls Complete directly (see BeginRoundAsync).
        On<EndCombatEvent>(e => Complete?.Invoke(e.Result));
        OnAsync<BeginCombatRoundEvent>(BeginRoundAsync);
        OnAsync<ObserveCombatEvent>(Observe);
        On<SelectCombatActionEvent>(OnSelectAction);
        On<SelectCombatTargetEvent>(OnSelectTarget);

        _groupId = groupId;
        Mobs = _mobs;

        // AttachChild(new UiFixedPositionElement(backgroundId, UiConstants.UiExtents));
        AttachChild(new Sprite(
            backgroundId,
            DrawLayer.Background,
            SpriteKeyFlags.NoTransform,
            SpriteFlags.LeftAligned)
        {
            Position = new Vector3(-1.0f, 1.0f, 1.0f),
            Size = new Vector2(2.0f, -2.0f)
        });
    }

    // Player selected an action from the context menu for a party member.
    void OnSelectAction(SelectCombatActionEvent e)
    {
        if (e.Action is CombatActionType.Attack or CombatActionType.Move)
        {
            PlanningState = CombatPlanningState.SelectingTarget;
            PendingActorPosition = e.ActorPosition;
            _pendingActionType = e.Action;
        }
        else
        {
            _plannedActions[e.ActorPosition] = new PlannedCombatAction(e.Action);
            PlanningState = CombatPlanningState.Planning;
            PendingActorPosition = -1;
        }
    }

    // Player clicked a tile while in SelectingTarget mode.
    void OnSelectTarget(SelectCombatTargetEvent e)
    {
        if (PlanningState != CombatPlanningState.SelectingTarget) return;
        
        // DEVIATION: In agent mode, bypass adjacency check so agent can plan attacks on any tile
        // (original game UI enforces range via context menu visibility, but agent should be able to plan freely)
        bool isValid = RaiseQuery(new IsAgentModeEvent()) || IsValidTarget(PendingActorPosition, e.TargetTileIndex);
        if (!isValid) return;
        
        _plannedActions[PendingActorPosition] = new PlannedCombatAction(_pendingActionType, e.TargetTileIndex);
        PlanningState = CombatPlanningState.Planning;
        PendingActorPosition = -1;
    }

    // Close range = Chebyshev distance 1. Long range = any tile.
    // Move = exactly 1 tile (Chebyshev distance 1, source: Horneman COMACTS.C MOVE_COMACT).
    // DEVIATION: exact Get_close_range_targets adjacency mask (COMACTS.C) not available in Horneman source snapshot.
    bool IsValidTarget(int actorPos, int targetPos)
    {
        if (actorPos < 0 || actorPos >= _tiles.Length) return false;

        if (_pendingActionType == CombatActionType.Move)
            return IsAdjacent(actorPos, targetPos)
                && targetPos >= 0 && targetPos < _tiles.Length
                && _tiles[targetPos] == null;

        if (_pendingActionType != CombatActionType.Attack) return true;

        var actor = _tiles[actorPos];
        if (actor == null) return false;

        var rightHand = actor.Effective.Inventory.RightHand;
        if (!rightHand.Item.IsNone && rightHand.Item.Type == AssetType.Item)
        {
            var item = Assets.LoadItem(rightHand.Item);
            if (item?.TypeId == ItemType.LongRangeWeapon)
                return true; // ranged: any tile is valid
        }

        // Close range (melee or unarmed): Chebyshev distance <= 1
        return IsAdjacent(actorPos, targetPos);
    }

    static bool IsAdjacent(int pos1, int pos2)
    {
        int x1 = pos1 % SavedGame.CombatColumns, y1 = pos1 / SavedGame.CombatColumns;
        int x2 = pos2 % SavedGame.CombatColumns, y2 = pos2 / SavedGame.CombatColumns;
        return Math.Abs(x1 - x2) <= 1 && Math.Abs(y1 - y2) <= 1;
    }

AlbionTask Observe(ObserveCombatEvent _) =>
        WithFrozenClock(this, async x =>
        {
            if (RaiseQuery(new IsAgentModeEvent()))
                return;

            Raise(new CombatDialog.ShowCombatDialogEvent(false));
            var dlg = x.AttachChild(new InvisibleWaitForClickDialog());
            await dlg.Task;
            Raise(new CombatDialog.ShowCombatDialogEvent(true));
        });

    // Round execution — source: Horneman COMBAT.C Create_sorted_combat_event_list + COMACTS.C actions
    AlbionTask BeginRoundAsync(BeginCombatRoundEvent _) =>
        WithFrozenClock(this, async x =>
        {
            var rng = Resolve<IRandom>();

            // Speed-sorted order descending (Shellsort by Speed in Horneman)
            var roundOrder = _mobs
                .Where(p => !p.IsDead)
                .OrderByDescending(p => p.Effective.Attributes.Speed.Current)
                .ToList();

            foreach (var actor in roundOrder)
            {
                if (actor.IsDead) continue;

                Raise(new LogEvent(LogLevel.Info, $"[ROUND] actor={actor.SheetId} type={actor.Effective.Type} pos={actor.CombatPosition} hp={actor.Effective.Combat.LifePoints.Current}"));

                // Horneman COMBAT.C Attacks_per_round = Character_data.ActionPoints
                int actions = actor.Effective.Combat.ActionPoints;
                for (int i = 0; i < actions; i++)
                {
                    if (actor.IsDead) break;

                    Raise(new LogEvent(LogLevel.Info, $"  [ACTION] {i+1}/{actions} for {actor.SheetId}"));
                    if (actor.Effective.Type == CharacterType.Party)
                        ExecutePartyAction(actor, rng);
                    else
                        ExecuteMonsterAction(actor, rng);
                }
            }

            _plannedActions.Clear();
            PlanningState = CombatPlanningState.Planning;
            PendingActorPosition = -1;

            await RaiseA(new CombatUpdateEvent(10));

            bool anyMonsters = _mobs.Any(p => p.Effective.Type == CharacterType.Monster);
            bool anyParty    = _mobs.Any(p => p.Effective.Type == CharacterType.Party);

            Raise(new LogEvent(LogLevel.Info,
                $"[ROUND END] mobs={_mobs.Count} anyMonsters={anyMonsters} anyParty={anyParty} " +
                string.Join(", ", _mobs.Select(m => $"{m.SheetId}({m.Effective.Type},dead={m.IsDead})"))));

            if (!anyMonsters)
            {
                DistributeXp();
                Raise(new LogEvent(LogLevel.Info, $"[COMBAT END] Victory — XP={XpShare} gold={_apresGold}"));
                Raise(new CombatDialog.ShowCombatDialogEvent(false));
                var dlg = x.AttachChild(new ApresCombatDialog(XpShare, _apresGold, _apresFood, _apresItems));
                await dlg.Task;
                dlg.Remove();
                Raise(new LogEvent(LogLevel.Info, "[COMBAT END] Raising EndCombatEvent Victory"));
                // Use Raise (not Enqueue) so CombatDialog receives it immediately.
                // Invoke Complete directly because Exchange skips Battle's own On<EndCombatEvent> (sender == Battle).
                Raise(new EndCombatEvent(CombatResult.Victory));
                Complete?.Invoke(CombatResult.Victory);
            }
            else if (!anyParty)
            {
                Raise(new LogEvent(LogLevel.Info, "[COMBAT END] Raising EndCombatEvent PartyKilled"));
                Raise(new EndCombatEvent(CombatResult.PartyKilled));
                Complete?.Invoke(CombatResult.PartyKilled);
            }
        });

    void ExecutePartyAction(ICombatParticipant actor, IRandom rng)
    {
        if (!_plannedActions.TryGetValue(actor.CombatPosition, out var plan))
            return; // no action planned — skip turn

        switch (plan.ActionType)
        {
            case CombatActionType.None:
                break;

            case CombatActionType.Attack:
            {
                var target = plan.TargetTileIndex >= 0 ? GetTile(plan.TargetTileIndex) : null;
                if (target == null || target.IsDead) break;
Raise(new CombatAnimationEvent(actor.CombatPosition, CombatAnimationType.Attack, target.CombatPosition));
                var dmg = DamageCalculator.CalculateAfflictedDamage(rng, actor.Effective, target.Effective);
                ApplyDamageAndCleanup(target, dmg.Afflicted);
                Raise(new CombatDamageFloaterEvent(target.CombatPosition, dmg.Afflicted));
                Raise(new LogEvent(LogLevel.Info,
                    $"[DAMAGE] {actor.SheetId}→{target.SheetId}: raw={dmg.RawDamage} prot={dmg.RawProtection} rolled={dmg.RolledDamage} vs {dmg.RolledProtection} = {dmg.Afflicted}{(dmg.IsCritical ? " CRITICAL" : "")}"));
                break;
            }

            case CombatActionType.Move:
            {
                int newPos = plan.TargetTileIndex;
                if (newPos < 0 || newPos >= _tiles.Length || _tiles[newPos] != null) break;
                Raise(new CombatAnimationEvent(actor.CombatPosition, CombatAnimationType.Move));
                MoveParticipant(actor, newPos);
                break;
            }

            case CombatActionType.Flee:
            {
                // Horneman COMACTS.C Flee_combat_action — Flucht nur aus hinterster Reihe
                int row = actor.CombatPosition / SavedGame.CombatColumns;
                int lastRow = SavedGame.CombatRows - 1; // = 4
                if (row != lastRow)
                    break;

Raise(new CombatAnimationEvent(actor.CombatPosition, CombatAnimationType.Flee));
                _mobs.Remove(actor);
                if (actor.CombatPosition >= 0)
                    _tiles[actor.CombatPosition] = null;
                break;
            }

            case CombatActionType.CastSpell:
            case CombatActionType.UseMagicItem:
                // TODO: spell execution — stub for now
                break;
        }
    }

    // Monsters move toward party and attack if in melee range.
    void ExecuteMonsterAction(ICombatParticipant actor, IRandom rng)
    {
        Raise(new LogEvent(LogLevel.Info, $"  [MONSTER] {actor.SheetId} at tile {actor.CombatPosition} AP={actor.Effective.Combat.ActionPoints}"));
        // Horneman MONLOGIC.C Default_decider — Flucht-Entscheidung vor Aktion
        if (ShouldMonsterFlee(actor))
        {
            Raise(new LogEvent(LogLevel.Info, $"  [MONSTER] {actor.SheetId} FLEEING (danger>morale)"));
            Raise(new CombatAnimationEvent(actor.CombatPosition, CombatAnimationType.Flee));
            int row = actor.CombatPosition / SavedGame.CombatColumns;
            if (row == 0)
            {
                _mobs.Remove(actor);
                _tiles[actor.CombatPosition] = null;
            }
            else
            {
                MoveMonsterToward(actor, actor.CombatPosition % SavedGame.CombatColumns); // retreat toward row 0
            }
            Raise(new LogEvent(LogLevel.Info, $"{actor.SheetId} versucht zu fliehen!"));
            return;
        }

        var target = _mobs
            .Where(p => !p.IsDead && p.Effective.Type == CharacterType.Party)
            .OrderBy(p => Math.Abs(p.CombatPosition % SavedGame.CombatColumns - actor.CombatPosition % SavedGame.CombatColumns))
            .ThenBy(p => p.CombatPosition)
            .FirstOrDefault();

        Raise(new LogEvent(LogLevel.Info, $"  [MONSTER TARGET] {actor.SheetId} found target: {(target == null ? "none" : target.SheetId.ToString())} at tile {(target == null ? -1 : target.CombatPosition)}"));

        if (target == null) return;

        if (IsInMeleeRange(actor.CombatPosition, target.CombatPosition))
        {
            Raise(new LogEvent(LogLevel.Info, $"  [MONSTER ATTACK] {actor.SheetId} → {target.SheetId}"));
            Raise(new CombatAnimationEvent(actor.CombatPosition, CombatAnimationType.Attack, target.CombatPosition));
            var dmg = DamageCalculator.CalculateAfflictedDamage(rng, actor.Effective, target.Effective);
            ApplyDamageAndCleanup(target, dmg.Afflicted);
            Raise(new CombatDamageFloaterEvent(target.CombatPosition, dmg.Afflicted));
            Raise(new LogEvent(LogLevel.Info,
                $"[DAMAGE] {actor.SheetId}→{target.SheetId}: raw={dmg.RawDamage} prot={dmg.RawProtection} rolled={dmg.RolledDamage} vs {dmg.RolledProtection} = {dmg.Afflicted}{(dmg.IsCritical ? " CRITICAL" : "")}"));
        }
        else
        {
            MoveMonsterToward(actor, target.CombatPosition);
        }
    }

    // DEVIATION: range check approximated from Horneman MONLOGIC.C
    static bool IsInMeleeRange(int actorTile, int targetTile)
    {
        int cols = SavedGame.CombatColumns;
        int dx = Math.Abs(actorTile % cols - targetTile % cols);
        int dy = Math.Abs(actorTile / cols - targetTile / cols);
        return dx <= 1 && dy <= 1;
    }

    // Horneman MONLOGIC.C Default_decider — Monster-Flucht basierend auf Morale
    bool ShouldMonsterFlee(ICombatParticipant actor)
    {
        int courage = actor.Effective.Combat.Morale;
        if (courage == 0) return false; // 0 = fights to the death

        int total = _mobs.Count(p => p.Effective.Type == CharacterType.Monster);
        int surviving = _mobs.Count(p => p.Effective.Type == CharacterType.Monster && !p.IsDead);
        int globalDanger = total > 0 ? 100 - (surviving * 100 / total) : 100;
        int maxHp = actor.Effective.Combat.LifePoints.Max;
        int curHp = actor.Effective.Combat.LifePoints.Current;
        int localDanger = maxHp > 0 ? 100 - (curHp * 100 / maxHp) : 100;
        int danger = (globalDanger + localDanger) / 2;
        Raise(new LogEvent(LogLevel.Info, $"  [FLEE] {actor.SheetId}: total={total} surviving={surviving} globalDanger={globalDanger} maxHp={maxHp} curHp={curHp} localDanger={localDanger} danger={danger} courage={courage} flee={danger >= courage}"));
        return danger >= courage;
    }

    void MoveMonsterToward(ICombatParticipant actor, int targetTile)
    {
        int cols = SavedGame.CombatColumns;
        int actorRow = actor.CombatPosition / cols;
        int actorCol = actor.CombatPosition % cols;
        int targetRow = targetTile / cols;
        int targetCol = targetTile % cols;

        int newRow = actorRow + Math.Sign(targetRow - actorRow);
        int newCol = actorCol;
        if (newRow == actorRow)
            newCol = actorCol + Math.Sign(targetCol - actorCol);

        int newPos = newRow * cols + newCol;
        if (newPos >= 0 && newPos < _tiles.Length && _tiles[newPos] == null)
            MoveParticipant(actor, newPos);
    }

    void ApplyDamageAndCleanup(ICombatParticipant target, int damage)
    {
        Raise(new CombatAnimationEvent(target.CombatPosition, CombatAnimationType.Hit));
        var oldHp = target.Effective.Combat.LifePoints.Current;
        target.TakeDamage(damage);
        var newHp = target.Effective.Combat.LifePoints.Current;
        Raise(new LogEvent(LogLevel.Info, $"Damage: {damage} | {target.SheetId}: {oldHp} → {newHp}/{target.Effective.Combat.LifePoints.Max}"));
        if (!target.IsDead) return;

        Raise(new CombatAnimationEvent(target.CombatPosition, CombatAnimationType.Death));
        _addedExperiencePoints += target.ExperienceReward;
        CollectLoot(target);
        _mobs.Remove(target);
        _corpses.Add(target);
        if (target.CombatPosition >= 0)
            _tiles[target.CombatPosition] = null;
        Raise(new LogEvent(LogLevel.Info, $"Monster killed! XP reward: {_addedExperiencePoints}"));
    }

    // Source: Horneman COMBAT.C Kill_participant — collect items/gold/food from dead monsters.
    void CollectLoot(ICombatParticipant target)
    {
        var loot = target.GetLoot();
        if (loot == null) return;

        _apresGold += loot.Gold?.Amount ?? 0;
        _apresFood += loot.Rations?.Amount ?? 0;

        foreach (var slot in loot.EnumerateAll())
        {
            if (slot.Item.IsNone || slot.Item.Type != AssetType.Item) continue;
            _apresItems.Add((slot.Item, slot.Amount));
        }
    }

    void MoveParticipant(ICombatParticipant participant, int newPos)
    {
        int oldPos = participant.CombatPosition;
        if (oldPos >= 0 && oldPos < _tiles.Length)
            _tiles[oldPos] = null;
        _tiles[newPos] = participant;
        participant.SetCombatPosition(newPos);
    }

    // Source: Horneman COMBAT.C Added_experience_points distributed in Enter_Apres_combat
    void DistributeXp()
    {
        var party = Resolve<IParty>();
        var living = party.StatusBarOrder.Where(m => !m.IsDead).ToList();
        if (living.Count > 0 && _addedExperiencePoints > 0)
        {
            XpShare = _addedExperiencePoints / living.Count;
            foreach (var member in living)
                member.AddExperience(XpShare);
        }
    }

    protected override void Unsubscribed()
    {
        Exchange.Unregister(typeof(IReadOnlyBattle), this);
    }

    protected override void Subscribed()
    {
        Exchange.Register<IReadOnlyBattle>(this);

        if (_mobs.Count > 0)
            return;

        foreach (var partyMember in Resolve<IParty>().StatusBarOrder)
        {
            var pos = partyMember.CombatPosition;
            _mobs.Add(partyMember);
            if (pos >= 0 && pos < _tiles.Length)
                _tiles[pos] = partyMember;
        }

        var group = Assets.LoadMonsterGroup(_groupId);
        if (group == null)
        {
            Error($"Tried to start battle with group {_groupId}, but no such group was found.");
            Enqueue(new EndCombatEvent(CombatResult.Victory));
            return;
        }

        for (int row = 0; row < SavedGame.CombatRowsForMobs; row++)
        {
            for (int column = 0; column < SavedGame.CombatColumns; column++)
            {
                var index = row * SavedGame.CombatColumns + column;
                MonsterId mobId = group.Grid[index];
                if (mobId.IsNone)
                    continue;

                var monster = AttachChild(Resolve<IMonsterFactory>().BuildMonster(mobId, index));

                _mobs.Add(monster);
                _tiles[monster.CombatPosition] = monster;
            }
        }

        // DEVIATION: Monster stat randomisation (±5%) not implemented —
        // IEffectiveCharacterSheet is read-only and exposes no setters for attributes/HP.
        // Horneman COMBAT.C Clone_monster_data randomises all attributes at combat start.
    }

    public ICombatParticipant GetTile(int x, int y)
    {
        int tileIndex = x + y * SavedGame.CombatColumns;
        return GetTile(tileIndex);
    }

    public ICombatParticipant GetTile(int tileIndex)
        => tileIndex < 0 || tileIndex >= _tiles.Length ? null : _tiles[tileIndex];
}
