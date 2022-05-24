using UAlbion.Api.Eventing;
using UAlbion.Formats.Ids;
using UAlbion.Formats.MapEvents;

namespace UAlbion.Game.Events;

public class SetFlagArrayEvent : Event, IVerboseEvent
{
    public FlagArray Type { get; }
    public MapId MapId { get; }
    public int Number { get; }
    public SwitchOperation Operation { get; }
    public SetFlagArrayEvent(FlagArray type, MapId mapId, int number, SwitchOperation operation)
    {
        Type = type;
        MapId = mapId;
        Number = number;
        Operation = operation;
    }
}