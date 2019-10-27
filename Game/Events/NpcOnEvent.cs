using UAlbion.Api;
using UAlbion.Formats.AssetIds;

namespace UAlbion.Game.Events
{
    [Event("npc_on")]
    public class NpcOnEvent : Event, INpcEvent
    {
        public NpcOnEvent(NpcCharacterId npcId) { NpcId = npcId; }
        [EventPart("npcId")] public NpcCharacterId NpcId { get; }
    }
}
