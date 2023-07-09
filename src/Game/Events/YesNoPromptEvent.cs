using UAlbion.Api.Eventing;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Ids;

namespace UAlbion.Game.Events;

[Event("prompt:yes_no")]
public class YesNoPromptEvent : Event, IAsyncEvent<bool>, IVerboseEvent
{
    public YesNoPromptEvent(TextId textId) => StringId = new StringId(textId);

    [EventConstructor]
    public YesNoPromptEvent(StringId stringId) => StringId = stringId;
    [EventPart("id")] public StringId StringId { get; }
}