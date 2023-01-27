using UAlbion.Api.Eventing;
using UAlbion.Formats.Assets.Maps;

namespace UAlbion.Game.Entities.Map2D;

public class TriggerMapTileEvent : Event, IVerboseEvent
{
    public TriggerMapTileEvent(TriggerType type, int x, int y)
    {
        Type = type;
        X = x;
        Y = y;
    }

    public TriggerType Type { get; }
    public int X { get; }
    public int Y { get; }
}