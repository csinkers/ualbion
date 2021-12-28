using UAlbion.Api;
using UAlbion.Formats.Assets;

namespace UAlbion.Game.Veldrid.Assets;

[Event("iso_lab")]
public class IsoLabDeltaEvent : Event, IVerboseEvent
{
    public IsoLabDeltaEvent(int delta) => Delta = delta;
    [EventPart("delta")] public int Delta { get; }
}

public class IsoLabEvent : Event, IVerboseEvent
{
    public IsoLabEvent(LabyrinthId id) => Id = id;
    public LabyrinthId Id { get; }
}