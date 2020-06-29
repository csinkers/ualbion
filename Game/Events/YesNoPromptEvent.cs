using UAlbion.Api;
using UAlbion.Formats.AssetIds;

namespace UAlbion.Game.Events
{
    [Event("prompt:yes_no")]
    public class YesNoPromptEvent : IAsyncEvent<bool>
    {
        public YesNoPromptEvent(AssetType type, ushort id, int subId) : this(new StringId(type, id, subId)) { }
        public YesNoPromptEvent(StringId stringId) => StringId = stringId;
        [EventPart("type")] public AssetType Type => StringId.Type;
        [EventPart("id")] public ushort Id => StringId.Id;
        [EventPart("sub_id")] public int SubId => StringId.SubId;

        public StringId StringId { get; }
    }
}