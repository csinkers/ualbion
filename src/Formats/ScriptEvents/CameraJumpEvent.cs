using UAlbion.Api;

namespace UAlbion.Formats.ScriptEvents
{
    [Event("camera_jump", "Teleports the camera to the given position.")] // USED IN SCRIPT
    public class CameraJumpEvent : Event
    {
        public CameraJumpEvent(int x, int y) { X = x; Y = y; }
        [EventPart("x ")] public int X { get; }
        [EventPart("y")] public int Y { get; }
    }
}
