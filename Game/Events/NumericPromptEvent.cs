using UAlbion.Api;
using UAlbion.Formats.AssetIds;

namespace UAlbion.Game.Events
{
    [Event("prompt:numeric")]
    public class NumericPromptEvent : IAsyncEvent<int>
    {
        public NumericPromptEvent(AssetType stringType, ushort stringId, int stringSubId, int min, int max)
            : this(new StringId(stringType, stringId, stringSubId), min, max) { }

        public NumericPromptEvent(StringId text, int min, int max)
        {
            Text = text;
            Min = min;
            Max = max;
        }

        public StringId Text { get; }

        [EventPart("type")] public AssetType StringType => Text.Type;
        [EventPart("id")] public ushort StringId => Text.Id;
        [EventPart("sub_id")] public int StringSubId => Text.SubId;
        [EventPart("min")] public int Min { get; }
        [EventPart("max")] public int Max { get; }
    }
}