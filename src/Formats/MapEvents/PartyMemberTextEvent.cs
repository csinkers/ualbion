using UAlbion.Api;

namespace UAlbion.Formats.MapEvents
{
    [Event("party_member_text")]
    public class PartyMemberTextEvent : Event, IAsyncEvent
    {
        [EventPart("member_id")] public TextLocation Location { get; }
        [EventPart("text_id")] public byte TextId { get; }
        public PartyMemberTextEvent(TextLocation location, byte textId)
        {
            Location = location;
            TextId = textId;
        }
    }
}
