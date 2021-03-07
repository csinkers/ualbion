using UAlbion.Api;

namespace UAlbion.Formats.ScriptEvents
{
    [Event("npc_off")] // USED IN SCRIPT
    public class NpcOffEvent : Event
    {
        public NpcOffEvent(int npcId) { NpcId = npcId; }
        [EventPart("npcId")] public int NpcId { get; }
    }
}
