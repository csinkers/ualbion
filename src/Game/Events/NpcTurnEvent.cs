using UAlbion.Api;
using UAlbion.Formats;

namespace UAlbion.Game.Events
{
    [Event("npc_turn")]
    public class NpcTurnEvent : Event
    {
        public NpcTurnEvent(int npcId, Direction direction) { NpcId = npcId; Direction = direction; }
        [EventPart("npcId ")] public int NpcId { get; }
        [EventPart("direction")] public Direction Direction { get; }
    }
}
