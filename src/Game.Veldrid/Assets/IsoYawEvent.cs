using UAlbion.Api.Eventing;

namespace UAlbion.Game.Veldrid.Assets;

[Event("iso_yaw")]
public class IsoYawEvent : Event, IVerboseEvent
{
    public IsoYawEvent(float delta) => Delta = delta;
    [EventPart("delta")] public float Delta { get; }
}