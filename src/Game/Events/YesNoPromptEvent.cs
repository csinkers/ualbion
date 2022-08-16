using UAlbion.Api.Eventing;
using UAlbion.Formats.Assets;

namespace UAlbion.Game.Events;

[Event("prompt:yes_no")]
public class YesNoPromptEvent : Event, IAsyncEvent<bool>, IVerboseEvent
{
    public YesNoPromptEvent(StringId stringId) => StringId = stringId;
    [EventPart("id")] public StringId StringId { get; }
}