using UAlbion.Api;

namespace UAlbion.Game.Events
{
    [Event("camera_jump", "Teleports the camera to the given position.")]
    public class CameraJumpEvent : GameEvent
    {
        public CameraJumpEvent(int x, int y) { X = x; Y = y; }
        [EventPart("x ")] public int X { get; }
        [EventPart("y")] public int Y { get; }
    }
}