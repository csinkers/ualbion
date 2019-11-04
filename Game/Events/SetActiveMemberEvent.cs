using UAlbion.Api;
using UAlbion.Formats.AssetIds;

namespace UAlbion.Game.Events
{
    [Event("set_active_member")]
    public class SetActiveMemberEvent : GameEvent
    {
        public SetActiveMemberEvent(PartyCharacterId memberId) { MemberId = memberId; }
        [EventPart("memberid")] public PartyCharacterId MemberId { get; }
    }
}