using System;
using System.Globalization;
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
            if (parts == null) throw new ArgumentNullException(nameof(parts));
            int memberId = int.Parse(parts[1], CultureInfo.InvariantCulture);
            byte textId = byte.Parse(parts[2], CultureInfo.InvariantCulture);
            return new PartyMemberTextEvent(textId, memberId == 0 ? null : (PartyCharacterId?)(memberId-1));
        }

        public PartyMemberTextEvent(byte textId, PartyCharacterId? portraitId)
        {
            TextId = textId;
            MemberId = portraitId;
        }
    }
}
