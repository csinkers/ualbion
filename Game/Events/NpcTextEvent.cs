using UAlbion.Api;

namespace UAlbion.Game.Events
{
    [Event("npc_text")]
    public class NpcTextEvent : Event
    {
        public NpcTextEvent(int npcId, int textId) { NpcId = npcId; TextId = textId; }
        [EventPart("npcId ")] public int NpcId { get; }
        [EventPart("textId")] public int TextId { get; }
    }
}
