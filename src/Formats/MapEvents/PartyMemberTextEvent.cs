using UAlbion.Api;
using UAlbion.Formats.Assets;

namespace UAlbion.Formats.MapEvents
{
    [Event("party_member_text")]
    public class PartyMemberTextEvent : Event, IAsyncEvent
    {
        [EventPart("text_id")] public byte TextId { get; }
        [EventPart("member_id")] public PartyMemberId? MemberId { get; }
        public PartyMemberTextEvent(byte textId, PartyMemberId? portraitId)
        {
            TextId = textId;
            MemberId = portraitId;
        }
    }
}
