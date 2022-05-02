using UAlbion.Api.Eventing;

namespace UAlbion.Formats.ScriptEvents;

[Event("show_map")] // USED IN SCRIPT
public class ShowMapEvent : Event
{
    public ShowMapEvent(bool? show = null) => Show = show;
    [EventPart("show", true)] public bool? Show { get; }
}