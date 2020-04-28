using UAlbion.Api;
using UAlbion.Formats.AssetIds;

namespace UAlbion.Formats.MapEvents
{
    [Event("party_member_text")]
    public class PartyMemberTextEvent : TextEvent
    {
        public static PartyMemberTextEvent Parse(string[] parts)
        {
            int portraitId = int.Parse(parts[1]);
            byte textId = byte.Parse(parts[2]);
            return new PartyMemberTextEvent(textId, (SmallPortraitId)portraitId);
        }

        public PartyMemberTextEvent(byte textId, SmallPortraitId portraitId) : base(textId, null, portraitId) { }
    }
}