using UAlbion.Api.Eventing;
using UAlbion.Formats.Ids;

namespace UAlbion.Game.Events;

[Event("use_magic", "Opens spell selection for a party member outside of combat")]
public record UseOutOfCombatSpellEvent(
    [property: EventPart("caster")] PartyMemberId CasterId) : EventRecord;
