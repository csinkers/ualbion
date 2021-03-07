using UAlbion.Api;

namespace UAlbion.Formats.ScriptEvents
{
    [Event("npc_lock")] // USED IN SCRIPT
    public class NpcLockEvent : Event
    {
        public NpcLockEvent(int npcId) { NpcId = npcId; }
        [EventPart("npcId")] public int NpcId { get; }
    }
}
