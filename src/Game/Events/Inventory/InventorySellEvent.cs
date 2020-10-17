using UAlbion.Api;
using UAlbion.Formats.Assets;

namespace UAlbion.Game.Events.Inventory
{
    [Event("inv:sell")]
    public class InventorySellEvent : InventorySlotEvent
    {
        public InventorySellEvent(InventoryId inventoryId, ItemSlotId slotId) 
            : base(inventoryId, slotId) { }
    }
}