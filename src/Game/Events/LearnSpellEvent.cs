using UAlbion.Api.Eventing;
using UAlbion.Formats.Ids;

namespace UAlbion.Game.Events;

#pragma warning disable CA1801 // Shouldn't be needed - https://github.com/dotnet/roslyn-analyzers/issues/4397
[Event("learn_spell")]
public record LearnSpellEvent(
    [property:EventPart("target")] SheetId Target,
    [property:EventPart("spell")] SpellId Spell) : EventRecord;
#pragma warning restore CA1801