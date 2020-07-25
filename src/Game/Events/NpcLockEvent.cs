using UAlbion.Api;

namespace UAlbion.Game.Events
{
    [Event("npc_lock")]
    public class NpcLockEvent : Event
    {
        public NpcLockEvent(int npcId) { NpcId = npcId; }
        [EventPart("npcId")] public int NpcId { get; }
    }
}
