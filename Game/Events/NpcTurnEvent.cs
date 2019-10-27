using UAlbion.Api;
using UAlbion.Formats.AssetIds;

namespace UAlbion.Game.Events
{
    [Event("npc_turn")]
    public class NpcTurnEvent : Event, INpcEvent
    {
        public NpcTurnEvent(NpcCharacterId npcId, int direction) { NpcId = npcId; Direction = direction; }
        [EventPart("npcId ")] public NpcCharacterId NpcId { get; }
        [EventPart("direction")] public int Direction { get; }
    }
}
