using UAlbion.Api;
using UAlbion.Formats.AssetIds;

namespace UAlbion.Game.Events
{
    [Event("npc_jump", "Teleport the given NPC to the given position.")]
    public class NpcJumpEvent : Event, INpcEvent
    {
        public NpcJumpEvent(NpcCharacterId npcId, int? x, int? y) { NpcId = npcId; X = x; Y = y; }
        [EventPart("npcId ")] public NpcCharacterId NpcId { get; }
        [EventPart("x", true)] public int? X { get; }
        [EventPart("y", true)] public int? Y { get; }
    }
}
