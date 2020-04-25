using UAlbion.Api;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Assets;

namespace UAlbion.Game.Events
{
    [Event("inv:pickup", "Pickup one or more items from a party member's equipped items or inventory and/or drop the currently held item(s)'")]
    public class InventoryPickupItemEvent : GameEvent
    {
        public InventoryPickupItemEvent(AssetType sourceType, int sourceId, ItemSlotId slotId, int? quantity = null)
        {
            SourceType = sourceType;
            SourceId = sourceId;
            SlotId = slotId;
            Quantity = quantity;
        }

        [EventPart("source_type", "The type of inventory to take from")] public AssetType SourceType { get; }
        [EventPart("source_id", "The id of the inventory to take from")] public int SourceId { get; }
        [EventPart("slot", "The body or inventory slot to take from / give to. Defaults to the first empty inventory slot if not supplied.")] public ItemSlotId SlotId { get; }
        [EventPart("quantity", "The number of items in the slot to pick up. Defaults to all items if not supplied.", true)] public int? Quantity { get; }
    }
}
