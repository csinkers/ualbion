using UAlbion.Api;
using UAlbion.Config;
using UAlbion.Formats.Assets;

namespace UAlbion.Formats.MapEvents
{
    [Event("inv:merchant", "Opens the inventory screen for the given merchant")]
    public class MerchantEvent : Event, ISetInventoryModeEvent, IAsyncEvent
    {
        public MerchantEvent(MerchantId merchantId, PartyMemberId member) // If member is None, then open with the current team leader
        {
            MerchantId = merchantId;
            Member = member;
        }

        public InventoryMode Mode => InventoryMode.Merchant;
        public AssetId Submode => MerchantId;
        [EventPart("id")] public MerchantId MerchantId { get; }
        [EventPart("partyMember")] public PartyMemberId Member { get; }
        public ISetInventoryModeEvent CloneForMember(PartyMemberId member) => new MerchantEvent(MerchantId, member);
    }
}
