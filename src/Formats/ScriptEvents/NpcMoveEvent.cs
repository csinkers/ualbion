using UAlbion.Api.Eventing;
using UAlbion.Formats.MapEvents;

namespace UAlbion.Formats.ScriptEvents;

[Event("npc_move")] // USED IN SCRIPT
public class NpcMoveEvent : Event, INpcEvent
{
    public NpcMoveEvent(byte npcNum, int x, int y) { NpcNum = npcNum; X = x; Y = y; }
    [EventPart("npcNum ")] public byte NpcNum { get; }

    // Speed: how many tiles to move in 4 slow ticks
    [EventPart("x ")] public int X { get; } 
    [EventPart("y")] public int Y { get; }
}