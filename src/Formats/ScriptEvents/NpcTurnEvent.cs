using UAlbion.Api;
using UAlbion.Formats.MapEvents;

namespace UAlbion.Formats.ScriptEvents;

[Event("npc_turn")] // USED IN SCRIPT
public class NpcTurnEvent : Event, INpcEvent
{
    public NpcTurnEvent(byte npcNum, Direction direction) { NpcNum = npcNum; Direction = direction; }
    [EventPart("npcNum ")] public byte NpcNum { get; }
    [EventPart("direction", true, Direction.Unchanged)] public Direction Direction { get; }
}