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

    [Event("e:set_mag", "Sets the current magnification level")]
    public class SetCameraMagnificationEvent : EngineEvent
    {
        public SetCameraMagnificationEvent(float magnification)
        {
            Magnification = magnification;
        }

        [EventPart("magnification", "The magnification level")]
        public float Magnification { get; }
    }
}