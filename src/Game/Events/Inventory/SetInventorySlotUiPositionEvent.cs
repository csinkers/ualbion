using UAlbion.Api;
using UAlbion.Formats.Assets;

namespace UAlbion.Game.Events.Inventory
{
    public class SetInventorySlotUiPositionEvent : GameEvent, IVerboseEvent
    {
        public SetInventorySlotUiPositionEvent(InventoryType inventoryType, ushort id, ItemSlotId slot, int x, int y)
        {
            InventoryType = inventoryType;
            Id = id;
            Slot = slot;
            X = x;
            Y = y;
        }

        public InventoryType InventoryType { get; }
        public ushort Id { get; }
        public ItemSlotId Slot { get; }
        public int X { get; }
        public int Y { get; }
        public InventorySlotId InventorySlotId => new InventorySlotId(InventoryType, Id, Slot);
    }
}