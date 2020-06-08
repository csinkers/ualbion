using UAlbion.Api;
using UAlbion.Formats.Assets;

namespace UAlbion.Game.Events.Inventory
{
    [Event("inv:discard")]
    public class InventoryDiscardEvent : InventorySlotEvent
    {
        public InventoryDiscardEvent(InventoryType inventoryType, ushort id, ItemSlotId slotId) 
            : base(inventoryType, id, slotId) { }
    }
}