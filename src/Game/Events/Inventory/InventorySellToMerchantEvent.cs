using UAlbion.Api.Eventing;
using UAlbion.Formats.Assets.Inv;

namespace UAlbion.Game.Events.Inventory;

[Event("inv:sell_to_merchant")]
public class InventorySellToMerchantEvent : InventorySlotEvent
{
    public InventorySellToMerchantEvent(InventoryId inventoryId, ItemSlotId slotId)
        : base(inventoryId, slotId) { }
}
