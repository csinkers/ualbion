using UAlbion.Api;

namespace UAlbion.Formats.ScriptEvents
{
    [Event("camera_move", "Move the camera using relative tile coordinates.", "cam_move")] // USED IN SCRIPT
    public class CameraMoveEvent : Event, IVerboseEvent
    {
        public CameraMoveEvent(int x, int y, int? z) { X = x; Y = y; Z = z; }
        [EventPart("x ")] public int X { get; }
        [EventPart("y")] public int Y { get; }
        [EventPart("z', true")] public int? Z { get; }
    }
}
