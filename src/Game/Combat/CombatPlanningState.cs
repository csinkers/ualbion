namespace UAlbion.Game.Combat;

public enum CombatPlanningState
{
    Planning,        // players are selecting actions for party members
    SelectingTarget, // one member's action type chosen, now awaiting target tile click
}
