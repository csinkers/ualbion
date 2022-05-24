using UAlbion.Api.Eventing;
using UAlbion.Formats.Ids;

namespace UAlbion.Formats.MapEvents;

[Event("inv:merchant", "Opens the inventory screen for the given merchant")]
public class MerchantEvent : Event, IAsyncEvent
{
    public MerchantEvent(MerchantId merchantId, PartyMemberId member) // If member is None, then open with the current team leader
    {
        MerchantId = merchantId;
        PartyMemberId = member;
    }

    [EventPart("id")] public MerchantId MerchantId { get; }
    [EventPart("partyMember")] public PartyMemberId PartyMemberId { get; }
}