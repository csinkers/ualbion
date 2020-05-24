using UAlbion.Api;
using UAlbion.Formats.Assets;

namespace UAlbion.Game.Events.Inventory
{
    [Event("inv:pickup_drop", "Pickup one or more items from a party member's equipped items or inventory and/or drop the currently held item(s)'")]
    public class InventoryPickupDropEvent : InventorySlotEvent
    {
        public InventoryPickupDropEvent(InventoryType sourceType, int sourceId, ItemSlotId slotId)
            : base(sourceType, sourceId, slotId) { }
    }
}