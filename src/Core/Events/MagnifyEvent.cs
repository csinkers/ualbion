using UAlbion.Api.Eventing;

namespace UAlbion.Core.Events;

[Event("e:mag", "Changes the current magnification level.")]
public class MagnifyEvent : EngineEvent
{
    public MagnifyEvent(int delta) => Delta = delta;

    [EventPart("delta", "The change in magnification level")]
    public int Delta { get; }
}

[Event("cam_mag", "Sets the current magnification level")]
public class CameraMagnificationEvent : EngineEvent
{
    public CameraMagnificationEvent(float magnification) => Magnification = magnification;

    [EventPart("magnification", "The magnification level")]
    public float Magnification { get; }
}