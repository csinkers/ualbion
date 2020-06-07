﻿using UAlbion.Formats.AssetIds;

namespace UAlbion.Formats.Assets
{
    public class ReadOnlyItemSlot : IReadOnlyItemSlot
    {
        readonly ItemSlot _slot;
        public ReadOnlyItemSlot(ItemSlot slot) => _slot = slot;
        public ushort Amount => _slot.Amount;
        public byte Charges => _slot.Charges;
        public byte Enchantment => _slot.Enchantment;
        public ItemSlotFlags Flags => _slot.Flags;
        public IContents Item => _slot.Item;
        public ItemId? ItemId => _slot.ItemId;
    }
}