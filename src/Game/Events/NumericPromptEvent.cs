using UAlbion.Api;
using UAlbion.Formats.Assets;

namespace UAlbion.Game.Events
{
    [Event("prompt:numeric")]
    public class NumericPromptEvent : IAsyncEvent<int>
    {
        public NumericPromptEvent(StringId text, int min, int max)
        {
            Text = text;
            Min = min;
            Max = max;
        }

        [EventPart("id")] public StringId Text { get; }
        [EventPart("min")] public int Min { get; }
        [EventPart("max")] public int Max { get; }

        public string ToStringNumeric() => ToString();
    }
}