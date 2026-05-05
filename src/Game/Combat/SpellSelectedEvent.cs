using UAlbion.Api.Eventing;
using UAlbion.Formats.Ids;

namespace UAlbion.Game.Combat;

[Event("spell_selected", "A spell has been selected for a party member")]
public record SpellSelectedEvent(
    [property: EventPart("actorPosition")] int ActorPosition,
    [property: EventPart("spellId")] SpellId SpellId) : EventRecord;
