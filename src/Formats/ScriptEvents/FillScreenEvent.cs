using UAlbion.Api.Eventing;

namespace UAlbion.Formats.ScriptEvents;

[Event("fill_screen")] // USED IN SCRIPT
public class FillScreenEvent : Event
{
    public FillScreenEvent(int color) { Color = color; }
    [EventPart("color")] public int Color { get; }
}