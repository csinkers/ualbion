using UAlbion.Api;

namespace UAlbion.Core.Events
{
    public class SetCameraDirectionEvent : EngineEvent, IVerboseEvent
    {
        public SetCameraDirectionEvent(float yaw, float pitch)
        {
            Yaw = yaw;
            Pitch = pitch;
        }

        public float Yaw { get; }
        public float Pitch { get; }
    }
}
