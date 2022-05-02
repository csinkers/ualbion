using UAlbion.Api;
using UAlbion.Api.Eventing;

namespace UAlbion.Formats.ScriptEvents;

[Event("active_member_text", "Prompts the active party member to say something.")] // USED IN SCRIPT
public class ActiveMemberTextEvent : Event
{
    public ActiveMemberTextEvent(int textId) { TextId = textId; }
    [EventPart("textId", "The string / conversation identifier.")] public int TextId { get; }
}