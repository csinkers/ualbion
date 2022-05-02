using UAlbion.Api;
using UAlbion.Api.Eventing;
using UAlbion.Formats.Assets;

namespace UAlbion.Game.Events;

[Event("prompt:yes_no")]
public class YesNoPromptEvent : IAsyncEvent<bool>
{
    public YesNoPromptEvent(StringId stringId) => StringId = stringId;
    [EventPart("id")] public StringId StringId { get; }
    [EventPart("sub_id")] public int SubId => StringId.SubId;
    public string ToStringNumeric() => ToString();
}