using UAlbion.Api;

namespace UAlbion.Core.Events
{
    [Event("e:mag", "Changes the current magnification level.")]
    public class MagnifyEvent : EngineEvent
    {
        public MagnifyEvent(int delta)
        {
            Delta = delta;
        }

        [EventPart("delta", "The change in magnification level")]
        public int Delta { get; }
    }

    public class SetCameraMagnificationEvent : EngineEvent
    {
        public SetCameraMagnificationEvent(float magnification)
        {
            Magnification = magnification;
        }

        public float Magnification { get; }
    }
}