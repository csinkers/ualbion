using UAlbion.Api;
using UAlbion.Core;
using UAlbion.Formats.Assets;

namespace UAlbion.Game.Events;

public class SetFlagArrayEvent : Event, IVerboseEvent
{
    public FlagArray Type { get; }
    public MapId MapId { get; }
    public int Number { get; }
    public FlagOperation Operation { get; }
    public SetFlagArrayEvent(FlagArray type, MapId mapId, int number, FlagOperation operation)
    {
        Type = type;
        MapId = mapId;
        Number = number;
        Operation = operation;
    }
}