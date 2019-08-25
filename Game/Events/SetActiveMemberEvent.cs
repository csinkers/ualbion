using UAlbion.Api;

namespace UAlbion.Game.Events
{
    [Event("set_active_member")]
    public class SetActiveMemberEvent : GameEvent
    {
        public SetActiveMemberEvent(int memberId) { MemberId = memberId; }
        [EventPart("memberid")] public int MemberId { get; }
    }
}