﻿using UAlbion.Api;
using UAlbion.Formats.Assets;

namespace UAlbion.Game.Events.Inventory
{
    [Event("inv:swap", "Swap the item in hand (if any) with the given inventory slot. May pickup, drop, coalesce or swap items.")]
    public class InventorySwapEvent : InventorySlotEvent
    {
        public InventorySwapEvent(InventoryType sourceType, ushort sourceId, ItemSlotId slotId)
            : base(sourceType, sourceId, slotId) { }
    }
}
