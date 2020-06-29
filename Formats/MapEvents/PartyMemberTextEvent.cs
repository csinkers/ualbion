using UAlbion.Api;
using UAlbion.Formats.AssetIds;

namespace UAlbion.Formats.MapEvents
{
    [Event("party_member_text")]
    public class PartyMemberTextEvent : Event, IAsyncEvent
    {
        [EventPart("member_id")] public PartyCharacterId? MemberId { get; }
        [EventPart("text_id")] public byte TextId { get; }

        public static PartyMemberTextEvent Parse(string[] parts)
        {
            int memberId = int.Parse(parts[1]);
            byte textId = byte.Parse(parts[2]);
            return new PartyMemberTextEvent(textId, (PartyCharacterId?)memberId);
        }

        public PartyMemberTextEvent(byte textId, PartyCharacterId? portraitId)
        {
            TextId = textId;
            MemberId = portraitId;
        }
    }
}