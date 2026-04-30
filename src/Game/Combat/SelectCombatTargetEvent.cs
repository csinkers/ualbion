using UAlbion.Api.Eventing;

namespace UAlbion.Game.Combat;

// Fired when the player clicks a tile while the battle is in SelectingTarget mode.
[Event("select_combat_target", "Select the target tile for the pending party member action")]
public record SelectCombatTargetEvent([property: EventPart("targetTileIndex")] int TargetTileIndex) : EventRecord;
