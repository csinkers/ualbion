using UAlbion.Api;
using UAlbion.Formats.AssetIds;

namespace UAlbion.Game.Events
{
    [Event("npc_move")]
    public class NpcMoveEvent : Event, INpcEvent
    {
        public NpcMoveEvent(NpcCharacterId npcId, int x, int y) { NpcId = npcId; X = x; Y = y; }
        [EventPart("npcId ")] public NpcCharacterId NpcId { get; }
        [EventPart("x ")] public int X { get; }
        [EventPart("y")] public int Y { get; }
    }
}
