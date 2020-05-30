using UAlbion.Api;
using UAlbion.Formats.Assets;

namespace UAlbion.Game.Events.Inventory
{
    public abstract class InventorySlotEvent : GameEvent, IInventoryEvent
    {
        protected InventorySlotEvent(InventoryType inventoryType, ushort inventoryId, ItemSlotId slotId)
        {
            InventoryType = inventoryType;
            InventoryId = inventoryId;
            SlotId = slotId;
        }

        [EventPart("source_type", "The type of inventory to discard from")] public InventoryType InventoryType { get; }
        [EventPart("source_id", "The id of the inventory to discard from")] public ushort InventoryId { get; }
        [EventPart("slot_id", "The slot in the inventory to discard from")] public ItemSlotId SlotId { get; }
    }
}