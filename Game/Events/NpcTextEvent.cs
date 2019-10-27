using UAlbion.Api;
using UAlbion.Formats.AssetIds;

namespace UAlbion.Game.Events
{
    [Event("npc_text")]
    public class NpcTextEvent : Event, INpcEvent
    {
        public NpcTextEvent(NpcCharacterId npcId, int textId) { NpcId = npcId; TextId = textId; }
        [EventPart("npcId ")] public NpcCharacterId NpcId { get; }
        [EventPart("textId")] public int TextId { get; }
    }
}
