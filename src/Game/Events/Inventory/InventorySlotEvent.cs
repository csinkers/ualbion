using UAlbion.Api.Eventing;
using UAlbion.Formats.Assets.Inv;

namespace UAlbion.Game.Events.Inventory;

public abstract class InventorySlotEvent : GameEvent, IInventoryEvent
{
    protected InventorySlotEvent(InventoryId id, ItemSlotId slotId)
    {
        Id = id;
        SlotId = slotId;
    }

    [EventPart("id", "The id of the inventory to discard from")] public InventoryId Id { get; }
    [EventPart("slot_id", "The slot in the inventory to discard from")] public ItemSlotId SlotId { get; }
}