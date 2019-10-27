using UAlbion.Api;
using UAlbion.Formats.AssetIds;

namespace UAlbion.Game.Events
{
    [Event("npc_off")]
    public class NpcOffEvent : Event, INpcEvent
    {
        public NpcOffEvent(NpcCharacterId npcId) { NpcId = npcId; }
        [EventPart("npcId")] public NpcCharacterId NpcId { get; }
    }
}
