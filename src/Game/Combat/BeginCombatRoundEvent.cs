using UAlbion.Api.Eventing;

namespace UAlbion.Game.Combat;

[Event("begin_combat_round", "Begin the next combat round")]
public record BeginCombatRoundEvent : EventRecord;