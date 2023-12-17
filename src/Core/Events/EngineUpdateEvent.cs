using UAlbion.Api.Eventing;

namespace UAlbion.Core.Events;

[Event("e:update")]
public class EngineUpdateEvent : EngineEvent, IVerboseEvent
{
    public EngineUpdateEvent(float deltaSeconds) { DeltaSeconds = deltaSeconds; }
    [EventPart("delta_seconds")] public float DeltaSeconds { get; set; }
}