using UAlbion.Api;

namespace UAlbion.Game.Events
{
    [Event("npc_unlock")] // USED IN SCRIPT
    public class NpcUnlockEvent : Event
    {
        public NpcUnlockEvent(int npcId) { NpcId = npcId; }
        [EventPart("npcId")] public int NpcId { get; }
    }
}
