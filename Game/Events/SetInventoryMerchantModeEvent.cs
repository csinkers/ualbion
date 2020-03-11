using UAlbion.Api;
using UAlbion.Formats.AssetIds;
using UAlbion.Game.Gui.Inventory;

namespace UAlbion.Game.Events
{
    [Event("set_inv_merchant_mode", "Sets the current inventory mode")]
    public class SetInventoryMerchantModeEvent : GameEvent, ISetInventoryModeEvent
    {
        public SetInventoryMerchantModeEvent(MerchantId merchantId, PartyCharacterId member)
        {
            MerchantId = merchantId;
            Member = member;
        }

        public InventoryMode Mode => InventoryMode.Merchant;
        [EventPart("merchant")] public MerchantId MerchantId { get; }
        [EventPart("member")] public PartyCharacterId Member { get; }
    }
}
