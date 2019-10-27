using UAlbion.Api;
using UAlbion.Formats.AssetIds;

namespace UAlbion.Game.Events
{
    [Event("npc_unlock")]
    public class NpcUnlockEvent : Event, INpcEvent
    {
        public NpcUnlockEvent(NpcCharacterId npcId) { NpcId = npcId; }
        [EventPart("npcId")] public NpcCharacterId NpcId { get; }
    }
}
