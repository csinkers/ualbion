using UAlbion.Api;

namespace UAlbion.Game.Events
{
    [Event("npc_move")]
    public class NpcMoveEvent : Event, INpcEvent
    {
        public NpcMoveEvent(int npcId, int x, int y) { NpcId = npcId; X = x; Y = y; }
        [EventPart("npcId ")] public int NpcId { get; }
        [EventPart("x ")] public int X { get; }
        [EventPart("y")] public int Y { get; }
    }
}