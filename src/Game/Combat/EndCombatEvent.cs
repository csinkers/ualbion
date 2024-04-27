using UAlbion.Api.Eventing;

namespace UAlbion.Game.Combat;

[Event("end_combat", null, "ec")]
public record EndCombatEvent([property: EventPart("result")] CombatResult Result) : EventRecord;