using UAlbion.Api;

namespace UAlbion.Formats.ScriptEvents;

[Event("npc_turn")] // USED IN SCRIPT
public class NpcTurnEvent : Event
{
    public NpcTurnEvent(int npcId, Direction direction) { NpcId = npcId; Direction = direction; }
    [EventPart("npcId ")] public int NpcId { get; }
    [EventPart("direction", true, Direction.Unchanged)] public Direction Direction { get; }
}