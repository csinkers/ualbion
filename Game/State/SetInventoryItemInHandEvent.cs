using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Assets;
using UAlbion.Game.Events;

namespace UAlbion.Game.State
{
    public class SetInventoryItemInHandEvent : GameEvent
    {
        public SetInventoryItemInHandEvent(IHoldable itemInHand, AssetType inventoryType, int id, ItemSlotId slotId)
        {
            ItemInHand = itemInHand;
            InventoryType = inventoryType;
            Id = id;
            SlotId = slotId;
        }

        public IHoldable ItemInHand { get; }
        public AssetType InventoryType { get; }
        public int Id { get; }
        public ItemSlotId SlotId { get; }
    }
}