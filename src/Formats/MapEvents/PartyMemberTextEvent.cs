using UAlbion.Api;
using UAlbion.Formats.Assets;

namespace UAlbion.Formats.MapEvents
{
    [Event("party_member_text")] // USED IN SCRIPT
    public class PartyMemberTextEvent : Event, IAsyncEvent
    {
        [EventPart("member_id")] public PartyMemberId MemberId { get; }
        [EventPart("text_id")] public byte TextId { get; }
        public PartyMemberTextEvent(PartyMemberId memberId, byte textId)
        {
            MemberId = memberId;
            TextId = textId;
        }
    }
}
