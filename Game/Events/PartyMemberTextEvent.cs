using UAlbion.Api;

namespace UAlbion.Game.Events
{
    [Event("party_member_text")]
    public class PartyMemberTextEvent : GameEvent
    {
        public PartyMemberTextEvent(int partyMemberId, int textId) { PartyMemberId = partyMemberId; TextId = textId; }
        [EventPart("partyMemberId ")] public int PartyMemberId { get; }
        [EventPart("textId")] public int TextId { get; }
    }
}