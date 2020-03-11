using UAlbion.Api;

namespace UAlbion.Game.Events
{
    public class CameraRotateEvent : GameEvent, IVerboseEvent
    {
        public CameraRotateEvent(float yaw, float pitch) { Yaw = yaw; Pitch = pitch; }
        [EventPart("yaw ")] public float Yaw { get; }
        [EventPart("pitch")] public float Pitch { get; }
    }
}
