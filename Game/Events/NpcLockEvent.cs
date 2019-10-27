using UAlbion.Api;
using UAlbion.Formats.AssetIds;

namespace UAlbion.Game.Events
{
    [Event("npc_lock")]
    public class NpcLockEvent : Event, INpcEvent
    {
        public NpcLockEvent(NpcCharacterId npcId) { NpcId = npcId; }
        [EventPart("npcId")] public NpcCharacterId NpcId { get; }
    }
}
