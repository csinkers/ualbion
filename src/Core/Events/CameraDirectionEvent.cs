using UAlbion.Api;

namespace UAlbion.Core.Events;

[Event("cam_dir")]
public class CameraDirectionEvent : EngineEvent, IVerboseEvent
{
    public CameraDirectionEvent(float yaw, float pitch) { Yaw = yaw; Pitch = pitch; }
    [EventPart("yaw", "the yaw in degrees")] public float Yaw { get; }
    [EventPart("pitch", "the pitch in degrees")] public float Pitch { get; }
}