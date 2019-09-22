using UAlbion.Api;

namespace UAlbion.Core.Events
{
    [Event("set_cursor_position", "Sets the position of the mouse cursor")]
    public class SetCursorPositionEvent : EngineEvent, IVerboseEvent
    {
        [EventPart("x")] public int X { get; }
        [EventPart("y")] public int Y { get; }

        public SetCursorPositionEvent(int x, int y) { X = x; Y = y; }
    }
}