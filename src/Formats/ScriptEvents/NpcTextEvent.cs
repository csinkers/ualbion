using UAlbion.Api.Eventing;
using UAlbion.Formats.Ids;

namespace UAlbion.Formats.ScriptEvents;

[Event("npc_text")] // USED IN SCRIPT
public class NpcTextEvent : Event, IAsyncEvent
{
    public NpcTextEvent(NpcId npcId, byte textId)
    {
        TextId = textId;
        NpcId = npcId;
    }

    [EventPart("npc")] public NpcId NpcId { get; }
    [EventPart("text")] public byte TextId { get; }
}