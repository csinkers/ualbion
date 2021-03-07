using UAlbion.Api;

namespace UAlbion.Formats.ScriptEvents
{
    [Event("npc_on")] // USED IN SCRIPT
    public class NpcOnEvent : Event
    {
        public NpcOnEvent(int npcId) { NpcId = npcId; }
        [EventPart("npcId")] public int NpcId { get; }
    }
}
