using UAlbion.Api;

namespace UAlbion.Formats.ScriptEvents
{
    [Event("npc_jump", "Teleport the given NPC to the given position.")] // USED IN SCRIPT
    public class NpcJumpEvent : Event
    {
        public NpcJumpEvent(int npcId, int? x, int? y) { NpcId = npcId; X = x; Y = y; }
        [EventPart("npcId ")] public int NpcId { get; }
        [EventPart("x", true)] public int? X { get; }
        [EventPart("y", true)] public int? Y { get; }
    }
}
