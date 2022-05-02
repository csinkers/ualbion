using UAlbion.Api;
using UAlbion.Api.Eventing;

namespace UAlbion.Formats.ScriptEvents;

[Event("camera_jump", "Teleports the camera to the given position.")] // USED IN SCRIPT
public class CameraJumpEvent : Event
{
    public CameraJumpEvent(int x, int y, int? z = null) { X = x; Y = y; Z = z; }
    [EventPart("x ")] public int X { get; }
    [EventPart("y")] public int Y { get; }
    [EventPart("z", true)] public int? Z { get; }
}