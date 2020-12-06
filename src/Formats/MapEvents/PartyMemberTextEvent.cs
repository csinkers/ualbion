using UAlbion.Api;
using UAlbion.Formats.Assets;

namespace UAlbion.Formats.MapEvents
{
    [Event("party_member_text")]
    public class PartyMemberTextEvent : Event, IAsyncEvent
    {
        [EventPart("portrait_id")] public PortraitId PortraitId { get; }
        [EventPart("text_id")] public byte TextId { get; }
        public PartyMemberTextEvent(PortraitId portraitId, byte textId)
        {
            PortraitId = portraitId;
            TextId = textId;
        }
    }
}
