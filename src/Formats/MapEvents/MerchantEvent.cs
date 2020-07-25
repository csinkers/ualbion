using UAlbion.Api;
using UAlbion.Formats.AssetIds;

namespace UAlbion.Formats.MapEvents
{
    [Event("inv:merchant", "Opens the inventory screen for the given merchant")]
    public class MerchantEvent : Event, ISetInventoryModeEvent, IAsyncEvent
    {
        public MerchantEvent(MerchantId merchantId, PartyCharacterId? member)
        {
            MerchantId = merchantId;
            Member = member;
        }

        public InventoryMode Mode => InventoryMode.Merchant;
        public ushort Submode => (ushort)MerchantId;
        [EventPart("id")] public MerchantId MerchantId { get; }
        [EventPart("id", true)] public PartyCharacterId? Member { get; }
        public ISetInventoryModeEvent CloneForMember(PartyCharacterId member) => new MerchantEvent(MerchantId, member);
    }
}