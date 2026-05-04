using UAlbion.Api.Eventing;

namespace UAlbion.Game.Events;

[Event("party_turn_3d")]
public class PartyTurn3DEvent : Event, IVerboseEvent
{
    [EventPart("yaw")] public float YawDegrees { get; }
    public PartyTurn3DEvent(float yawDegrees)
    {
        YawDegrees = yawDegrees;
    }
}