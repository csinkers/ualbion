using UAlbion.Api;

namespace UAlbion.Game.Events
{
    [Event("camera_move", "Move the camera using relative coordinates.")]
    public class CameraMoveEvent : GameEvent
    {
        public CameraMoveEvent(int x, int y) { X = x; Y = y; }
        [EventPart("x ")] public int X { get; }
        [EventPart("y")] public int Y { get; }
    }
}