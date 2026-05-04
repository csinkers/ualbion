using System.Numerics;
using UAlbion.Api.Eventing;

namespace UAlbion.Game.Events;

[Event("party_move_3d")]
public class PartyMove3DEvent : Event, IVerboseEvent
{
    public Vector2 Velocity { get; set; }
    public float Yaw { get; set; }
    public float Pitch { get; set; }
}