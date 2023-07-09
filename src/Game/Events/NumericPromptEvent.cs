using UAlbion.Api.Eventing;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Ids;

namespace UAlbion.Game.Events;

[Event("prompt:numeric")]
public class NumericPromptEvent : Event, IAsyncEvent<int>
{
    public NumericPromptEvent(TextId textId, int min, int max) : this(new StringId(textId), min, max) { }

    [EventConstructor]
    public NumericPromptEvent(StringId text, int min, int max)
    {
        Text = text;
        Min = min;
        Max = max;
    }

    [EventPart("id")] public StringId Text { get; }
    [EventPart("min")] public int Min { get; }
    [EventPart("max")] public int Max { get; }
}