using UAlbion.Api;
using UAlbion.Formats.AssetIds;

namespace UAlbion.Formats.MapEvents
{
    [Event("start_party_dialogue", "Initiates a conversation using the given party member id")]
    public class StartPartyDialogueEvent : AsyncEvent
    {
        public StartPartyDialogueEvent(PartyCharacterId memberId) => MemberId = memberId;
        [EventPart("member_id")] public PartyCharacterId MemberId { get; }
        protected override AsyncEvent Clone() => new StartPartyDialogueEvent(MemberId);
    }
}