using UAlbion.Api.Eventing;
using UAlbion.Formats.Ids;

namespace UAlbion.Game.Events;

[Event("learn_spell")]
public record LearnSpellEvent([property:EventPart("target")] SheetId Target, [property:EventPart("spell")] SpellId Spell) : EventRecord;