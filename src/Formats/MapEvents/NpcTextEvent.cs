using System;
using UAlbion.Api;
using UAlbion.Formats.AssetIds;

namespace UAlbion.Formats.MapEvents
{
    [Event("npc_text")]
    public class NpcTextEvent : Event, IAsyncEvent
    {
        public NpcTextEvent(NpcCharacterId npcId, byte textId)
        {
            TextId = textId;
            NpcId = npcId;
        }

        [EventPart("npc")] public NpcCharacterId NpcId { get; }
        [EventPart("text")] public byte TextId { get; }
    }
}
