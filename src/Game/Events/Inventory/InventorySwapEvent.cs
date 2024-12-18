﻿using UAlbion.Api.Eventing;
using UAlbion.Formats.Assets.Inv;

namespace UAlbion.Game.Events.Inventory;

[Event("inv:swap", "Swap the item in hand (if any) with the given inventory slot. May pickup, drop, coalesce or swap items.")]
public class InventorySwapEvent : InventorySlotEvent
{
    public InventorySwapEvent(InventoryId sourceId, ItemSlotId slotId)
        : base(sourceId, slotId) { }
}