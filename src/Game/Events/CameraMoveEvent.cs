using UAlbion.Api;

namespace UAlbion.Game.Events
{
    [Event("camera_move", "Move the camera using relative tile coordinates.")] // USED IN SCRIPT
    public class CameraMoveEvent : GameEvent, IVerboseEvent
    {
        public CameraMoveEvent(int x, int y) { X = x; Y = y; }
        [EventPart("x ")] public int X { get; }
        [EventPart("y")] public int Y { get; }
    }
}
