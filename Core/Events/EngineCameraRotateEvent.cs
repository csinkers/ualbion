using UAlbion.Api;

namespace UAlbion.Core.Events
{
    [Event("e:camera_rotate", "Rotate the camera using relative pitch & yaw (in radians)")]
    public class EngineCameraRotateEvent : EngineEvent, IVerboseEvent
    {
        public EngineCameraRotateEvent(float yaw, float pitch) { Yaw = yaw; Pitch = pitch; }
        [EventPart("yaw ")] public float Yaw { get; }
        [EventPart("pitch")] public float Pitch { get; }
    }
}