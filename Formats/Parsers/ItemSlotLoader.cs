using System;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Assets;

namespace UAlbion.Formats.Parsers
{
    static class ItemSlotLoader
    {
        public static ItemSlot Translate(ItemSlot slot, ISerializer s)  // 6 per slot
        {
            slot ??= new ItemSlot { Amount = 0, Id = ItemId.Knife };
            s.UInt8(nameof(slot.Amount), () => slot.Amount, x => slot.Amount = x);
            s.UInt8(nameof(slot.Charges), () => slot.Charges, x => slot.Charges = x);
            s.UInt8(nameof(slot.Enchantment), () => slot.Enchantment, x => slot.Enchantment = x);
            s.EnumU8(nameof(slot.Flags), () => slot.Flags, x => slot.Flags = x, x => ((byte)x, x.ToString()));
            s.UInt16(nameof(slot.Id),
                () => (ushort) ((int) slot.Id >= 100 ? (int) slot.Id + 1 : (int) slot.Id),
                x => slot.Id = (ItemId)(x > 100 ? x - 1 : x));
            return slot;
        }

        public static Action<ISerializer> Read(Action<ItemSlot> setter) => s =>
        {
            var slot = Translate(null, s);
            setter(slot.Amount == 0 ? null : slot);
        };

        public static Action<ISerializer> Write(ItemSlot slot) => s => Translate(slot, s);
    }
}