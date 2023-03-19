using UAlbion.Api.Eventing;

namespace UAlbion.Core.Events;

[Event("e:show_hw_cursor", "Shows/hides the default windows cursor")]
public class ShowHardwareCursorEvent : EngineEvent
{
    public ShowHardwareCursorEvent(bool show) => Show = show;
    [EventPart("show", true, true)] public bool Show { get; }

}