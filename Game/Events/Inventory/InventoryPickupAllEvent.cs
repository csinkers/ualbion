using UAlbion.Api;
using UAlbion.Formats.Assets;

namespace UAlbion.Game.Events.Inventory
{
    [Event("inv:pickup_all", "Pickup entire stack of items from a party member's inventory")]
    public class InventoryPickupAllEvent : InventorySlotEvent
    {
        public InventoryPickupAllEvent(InventoryType sourceType, ushort sourceId, ItemSlotId slotId)
            : base(sourceType, sourceId, slotId) { }
    }
}
