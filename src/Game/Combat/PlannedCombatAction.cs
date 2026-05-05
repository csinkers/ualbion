using UAlbion.Formats.Ids;

namespace UAlbion.Game.Combat;

// Stores one party member's pre-planned action for the upcoming round.
// Source: Horneman COMACTS.C Combat_participant.Current_action + Target union
public record PlannedCombatAction(CombatActionType ActionType, int TargetTileIndex = -1, SpellId SpellId = default);
