using UAlbion.Api;
using UAlbion.Api.Eventing;

namespace UAlbion.Game.Veldrid.Assets;

[Event("iso_pitch")]
public class IsoPitchEvent : Event, IVerboseEvent
{
    public IsoPitchEvent(float delta) => Delta = delta;
    [EventPart("delta")] public float Delta { get; }
}