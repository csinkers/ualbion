using UAlbion.Api;
using UAlbion.Formats.Assets;

namespace UAlbion.Game.Events
{
    [Event("prompt:yes_no")]
    public class YesNoPromptEvent : IAsyncEvent<bool>
    {
        public YesNoPromptEvent(StringId stringId) => StringId = stringId;
        [EventPart("id")] public StringId StringId { get; }
        [EventPart("sub_id")] public int SubId => StringId.SubId;
    }
}