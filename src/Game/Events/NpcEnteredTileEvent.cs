using UAlbion.Api;
using UAlbion.Formats.Assets;

namespace UAlbion.Game.Events;

[Event("npc_entered_tile")]
public class NpcEnteredTileEvent : GameEvent, IVerboseEvent
{
    public NpcEnteredTileEvent(NpcId id, int x, int y)
    {
        Id = id;
        X = x;
        Y = y;
    }

    [EventPart("id")] public NpcId Id { get; }
    [EventPart("x")] public int X { get; }
    [EventPart("y")] public int Y { get; }
}