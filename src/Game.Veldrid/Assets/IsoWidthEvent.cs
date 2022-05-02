using UAlbion.Api;
using UAlbion.Api.Eventing;

namespace UAlbion.Game.Veldrid.Assets;

[Event("iso_width")]
public class IsoWidthEvent : Event
{
    public IsoWidthEvent(int delta) => Delta = delta;
    [EventPart("delta")] public int Delta { get; }
}