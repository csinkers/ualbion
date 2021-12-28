using UAlbion.Api;

namespace UAlbion.Core.Events;

[Event("cam_pos")]
public class CameraPositionEvent : EngineEvent
{
    public CameraPositionEvent(float x, float y, float z) { X = x; Y = y; Z = z; }
    [EventPart("x")] public float X { get; }
    [EventPart("y")] public float Y { get; }
    [EventPart("z")] public float Z { get; }
}