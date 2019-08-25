using UAlbion.Api;

namespace UAlbion.Game.Events
{
    [Event("npc_unlock")]
    public class NpcUnlockEvent : Event, INpcEvent
    {
        public NpcUnlockEvent(int npcId) { NpcId = npcId; }
        [EventPart("npcId")] public int NpcId { get; }
    }
}