using UAlbion.Api;
using UAlbion.Formats.Assets;

namespace UAlbion.Game.Events.Inventory
{
    [Event("inv:discard")]
    public class InventoryDiscardEvent : GameEvent, IInventoryEvent
    {
        public InventoryDiscardEvent(InventoryType inventoryType, int id, ItemSlotId slotId)
        {
            InventoryType = inventoryType;
            InventoryId = id;
            SlotId = slotId;
        }

        [EventPart("source_type", "The type of inventory to discard from")] public InventoryType InventoryType { get; }
        [EventPart("source_id", "The id of the inventory to discard from")] public int InventoryId { get; }
        [EventPart("slot_id", "The slot in the inventory to discard from")] public ItemSlotId SlotId { get; }
    }
}