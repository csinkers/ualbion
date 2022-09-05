using UAlbion.Api.Eventing;
using UAlbion.Formats.Assets.Maps;

namespace UAlbion.Game.Entities.Map2D;

public class TriggerMapTileEvent : Event, IVerboseEvent
{
    public TriggerMapTileEvent(TriggerTypes type, int x, int y)
    {
        Type = type;
        X = x;
        Y = y;
    }

    public TriggerTypes Type { get; }
    public int X { get; }
    public int Y { get; }
}