using UAlbion.Api;

namespace UAlbion.Formats.ScriptEvents
{
    [Event("camera_move", "Move the camera using relative tile coordinates.")] // USED IN SCRIPT
    public class CameraMoveEvent : Event, IVerboseEvent
    {
        public CameraMoveEvent(int x, int y) { X = x; Y = y; }
        [EventPart("x ")] public int X { get; }
        [EventPart("y")] public int Y { get; }
    }
}
