using UAlbion.Api;

namespace UAlbion.Game.Events;

[Event("cam_rotate")]
public class CameraRotateEvent : GameEvent, IVerboseEvent
{
    public CameraRotateEvent(float yaw, float pitch) { Yaw = yaw; Pitch = pitch; }
    [EventPart("yaw ")] public float Yaw { get; set; }
    [EventPart("pitch")] public float Pitch { get; set; }
}