using UAlbion.Api;

namespace UAlbion.Game.Events
{
    [Event("show_cursor", "Enables or disables the mouse cursor")]
    public class ShowCursorEvent : GameEvent
    {
        public ShowCursorEvent(bool show) { Show = show; }
        [EventPart("show")] public bool Show { get; }
    }
}
