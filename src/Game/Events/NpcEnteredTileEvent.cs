using UAlbion.Api;
using UAlbion.Formats.MapEvents;

namespace UAlbion.Game.Events;

[Event("npc_entered_tile")]
public class NpcEnteredTileEvent : GameEvent, INpcEvent, IVerboseEvent
{
    public NpcEnteredTileEvent(byte npcNum, int x, int y)
    {
        NpcNum = npcNum;
        X = x;
        Y = y;
    }

    [EventPart("npcNum")] public byte NpcNum { get; }
    [EventPart("x")] public int X { get; }
    [EventPart("y")] public int Y { get; }
}