using UAlbion.Api;

namespace UAlbion.Game.Events
{
    [Event("npc_on")]
    public class NpcOnEvent : Event, INpcEvent
    {
        public NpcOnEvent(int npcId) { NpcId = npcId; }
        [EventPart("npcId")] public int NpcId { get; }
    }
}