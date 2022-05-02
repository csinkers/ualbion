using UAlbion.Api.Eventing;
using UAlbion.Formats.Assets;

namespace UAlbion.Formats.MapEvents;

[Event("start_party_dialogue", "Initiates a conversation using the given party member id")]
public class StartPartyDialogueEvent : Event, IAsyncEvent
{
    public StartPartyDialogueEvent(PartyMemberId memberId) => MemberId = memberId;
    [EventPart("member_id")] public PartyMemberId MemberId { get; }
}