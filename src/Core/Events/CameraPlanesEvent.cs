using UAlbion.Api;
using UAlbion.Api.Eventing;

namespace UAlbion.Core.Events;

[Event("cam_planes")]
public class CameraPlanesEvent : EngineEvent
{
    public CameraPlanesEvent(float near, float far) { Near = near; Far = far; }
    [EventPart("near")] public float Near { get; }
    [EventPart("far")] public float Far { get; }
}