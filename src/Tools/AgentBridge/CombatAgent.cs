using System;
using System.Collections.Generic;
using System.Linq;
using UAlbion.Api.Eventing;
using UAlbion.Formats.Assets.Save;
using UAlbion.Game.Combat;
using UAlbion.Game.State;

namespace UAlbion.Tools.AgentBridge;

public class CombatAgentStrategy
{
    public bool Enabled { get; set; }

    // "weakest" | "nearest" | "strongest"
    public string AttackPriority { get; set; } = "weakest";

    // HP ratio 0..1; if actor HP / max_HP < threshold → use healing item (future)
    public float HealThreshold { get; set; }
}

/// <summary>
/// Autonomous combat engine.  Runs inside the game loop (BeginFrameEvent) and:
///   1. Selects Attack action for each party member when their turn comes up.
///   2. Picks the best target according to the configured strategy.
///   3. Triggers begin_combat_round once all active party members have a planned action.
///
/// Only acts when Strategy.Enabled == true.  Designed to work headless (no companion needed).
/// </summary>
public class CombatAgent : Component
{
    public CombatAgentStrategy Strategy { get; } = new();

    readonly HashSet<int> _plannedActors = [];
    bool _roundTriggered;

    public CombatAgent()
    {
        On<BeginFrameEvent>(_ => Tick());
    }

    void Tick()
    {
        if (!Strategy.Enabled) return;

        var battle = TryResolve<IReadOnlyBattle>();
        if (battle == null)
        {
            // Combat ended
            _plannedActors.Clear();
            _roundTriggered = false;
            return;
        }

        var logExchange = TryResolve<ILogExchange>();
        if (logExchange == null) return;

        var state = TryResolve<IGameState>();
        if (state == null) return;

        // Target selection phase — we already raised SelectCombatAction, now pick the tile
        if (battle.PlanningState == CombatPlanningState.SelectingTarget)
        {
            int target = FindBestTarget(battle);
            if (target >= 0)
                logExchange.EnqueueEvent(new SelectCombatTargetEvent(target));
            return;
        }

        // Round executing (anything that isn't Planning or SelectingTarget)
        if (battle.PlanningState != CombatPlanningState.Planning)
        {
            if (_roundTriggered)
            {
                _plannedActors.Clear();
                _roundTriggered = false;
            }
            return;
        }

        // ── Planning phase ────────────────────────────────────────────────────

        var partyPositions = GetActivePartyPositions(state, battle);
        if (partyPositions.Count == 0) return;

        // All party members have a planned action → start the round
        if (partyPositions.All(_plannedActors.Contains))
        {
            if (!_roundTriggered)
            {
                _roundTriggered = true;
                logExchange.EnqueueEvent(new BeginCombatRoundEvent());
            }
            return;
        }

        // New planning cycle started (not all actors done → reset stale round flag)
        _roundTriggered = false;

        // Plan for the current pending actor
        int pendingPos = battle.PendingActorPosition;
        if (pendingPos < 0) return;

        int partyMaxTile = SavedGame.CombatColumns * SavedGame.CombatRowsForParty;
        if (pendingPos >= partyMaxTile) return;

        if (_plannedActors.Contains(pendingPos)) return;

        var actor = battle.GetTile(pendingPos);
        if (actor == null || actor.IsDead)
        {
            _plannedActors.Add(pendingPos); // skip dead/missing actor
            return;
        }

        // Heal check (future: use potion when HP ratio < HealThreshold)
        // TODO: implement UseMagicItem action when heal item found in inventory

        _plannedActors.Add(pendingPos);
        logExchange.EnqueueEvent(new SelectCombatActionEvent(pendingPos, CombatActionType.Attack));
    }

    int FindBestTarget(IReadOnlyBattle battle)
    {
        int monsterStart = SavedGame.CombatColumns * SavedGame.CombatRowsForParty;
        int tileCount    = SavedGame.CombatRows * SavedGame.CombatColumns;

        ICombatParticipant? best = null;
        int bestTile = -1;

        for (int i = monsterStart; i < tileCount; i++)
        {
            var mob = battle.GetTile(i);
            if (mob == null || mob.IsDead) continue;

            if (best == null) { best = mob; bestTile = i; continue; }

            bool replace = Strategy.AttackPriority switch
            {
                "weakest"   => mob.Effective.Combat.LifePoints.Current < best.Effective.Combat.LifePoints.Current,
                "strongest" => mob.Effective.Combat.LifePoints.Current > best.Effective.Combat.LifePoints.Current,
                _           => i < bestTile // "nearest" = lowest tile index
            };

            if (replace) { best = mob; bestTile = i; }
        }

        return bestTile;
    }

    static List<int> GetActivePartyPositions(IGameState state, IReadOnlyBattle battle)
    {
        var result = new List<int>();
        foreach (var player in state.Party.WalkOrder)
        {
            var pos = state.GetCombatPositionForPlayer(player.Id);
            if (!pos.HasValue) continue;
            var participant = battle.GetTile(pos.Value);
            if (participant == null || participant.IsDead) continue;
            result.Add(pos.Value);
        }
        return result;
    }
}
