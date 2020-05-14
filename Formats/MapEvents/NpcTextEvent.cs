using UAlbion.Api;
using UAlbion.Formats.AssetIds;

namespace UAlbion.Formats.MapEvents
{
    [Event("npc_text")]
    public class NpcTextEvent : AsyncEvent
    {
        public NpcTextEvent(NpcCharacterId npcId, byte textId)
        {
            TextId = textId;
            NpcId = npcId;
        }

        [EventPart("npc")] public NpcCharacterId NpcId { get; }
        [EventPart("text")] public byte TextId { get; }

        protected override AsyncEvent Clone() => new NpcTextEvent(NpcId, TextId);
    }
}