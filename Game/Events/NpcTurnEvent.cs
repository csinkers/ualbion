using UAlbion.Api;

namespace UAlbion.Game.Events
{
    [Event("npc_turn")]
    public class NpcTurnEvent : Event
    {
        public NpcTurnEvent(int npcId, int direction) { NpcId = npcId; Direction = direction; }
        [EventPart("npcId ")] public int NpcId { get; }
        [EventPart("direction")] public int Direction { get; }
    }
}
