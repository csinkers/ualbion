using UAlbion.Api;
using UAlbion.Formats.Assets;

namespace UAlbion.Game.Events.Inventory
{
    [Event("inv:pickup_drop", "Pickup one or more items from a party member's equipped items or inventory and/or drop the currently held item(s)'")]
    public class InventoryPickupDropItemEvent : GameEvent, IInventoryEvent
    {
        public InventoryPickupDropItemEvent(InventoryType sourceType, int sourceId, ItemSlotId slotId, int? quantity = null)
        {
            InventoryType = sourceType;
            InventoryId = sourceId;
            SlotId = slotId;
            Quantity = quantity;
        }

        [EventPart("source_type", "The type of inventory to take from")]public InventoryType InventoryType { get; }
        [EventPart("source_id", "The id of the inventory to take from")]public int InventoryId { get; }
        [EventPart("slot", "The body or inventory slot to take from / give to. Defaults to the first empty inventory slot if not supplied.")] public ItemSlotId SlotId { get; }
        [EventPart("quantity", "The number of items in the slot to pick up. Defaults to all items if not supplied.", true)] public int? Quantity { get; }
    }
}
