using UAlbion.Api.Eventing;
using UAlbion.Formats.Ids;

namespace UAlbion.Formats.ScriptEvents;

[Event("npc_text")] // USED IN SCRIPT
public class NpcTextEvent : Event
{
    public NpcTextEvent(NpcSheetId npcId, byte textId)
    {
        TextId = textId;
        NpcId = npcId;
    }

    [EventPart("npc")] public NpcSheetId NpcId { get; }
    [EventPart("text")] public byte TextId { get; }
}