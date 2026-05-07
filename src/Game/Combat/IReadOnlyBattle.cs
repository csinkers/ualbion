using System.Collections.Generic;
using UAlbion.Game.State;

namespace UAlbion.Game.Combat;

public interface IReadOnlyBattle
{
    IReadOnlyList<ICombatParticipant> Mobs { get; }
    ICombatParticipant GetTile(int x, int y);
    ICombatParticipant GetTile(int tileIndex);
    CombatPlanningState PlanningState { get; }
    int PendingActorPosition { get; }
    PlannedCombatAction GetPlannedAction(int tileIndex);
    bool IsValidTarget(int targetTileIndex);
}