﻿using UAlbion.Api;
using UAlbion.Formats.Assets;

namespace UAlbion.Game.Events.Inventory
{
    [Event("inv:sell")]
    public class InventorySellEvent : InventorySlotEvent
    {
        public InventorySellEvent(InventoryType inventoryType, ushort id, ItemSlotId slotId) 
            : base(inventoryType, id, slotId) { }
    }
}