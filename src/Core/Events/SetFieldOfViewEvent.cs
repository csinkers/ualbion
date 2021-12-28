using UAlbion.Api;

namespace UAlbion.Core.Events;

[Event("fov", "Change the 3D camera's field of view")]
public class SetFieldOfViewEvent : EngineEvent
{
    public SetFieldOfViewEvent(float? degrees) { Degrees = degrees; }

    [EventPart("degrees", "The field of view, in degrees")]
    public float? Degrees { get; }
}