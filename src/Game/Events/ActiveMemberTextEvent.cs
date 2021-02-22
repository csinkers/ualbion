using UAlbion.Api;

namespace UAlbion.Game.Events
{
    [Event("active_member_text", "Prompts the active party member to say something.")] // USED IN SCRIPT
    public class ActiveMemberTextEvent : GameEvent
    {
        public ActiveMemberTextEvent(int textId) { TextId = textId; }
        [EventPart("textId", "The string / conversation identifier.")] public int TextId { get; }
    }
}
