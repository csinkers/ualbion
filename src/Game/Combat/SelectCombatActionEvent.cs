using UAlbion.Api.Eventing;

namespace UAlbion.Game.Combat;

// Fired when the player picks an action for a party member from the context menu.
// ActorPosition: tile index of the party member.
// For Attack/Move the system enters SelectingTarget mode; for Flee/None the action is stored immediately.
[Event("select_combat_action", "Select an action for a party member before the round starts")]
public record SelectCombatActionEvent(
    [property: EventPart("actorPosition")] int ActorPosition,
    [property: EventPart("action")] CombatActionType Action) : EventRecord;
