using System;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Assets;

namespace UAlbion.Formats.Parsers
{
    static class ItemSlotLoader
    {
        public static void Translate(ItemSlot slot, ISerializer s)  // 6 per slot
        {
            if(slot == null)
                slot = new ItemSlot { Amount = 0, Id = ItemId.Knife };

            s.UInt8("Amount", () => slot.Amount, x => slot.Amount = x);
            s.UInt8("Charges", () => slot.Charges, x => slot.Charges = x);
            s.UInt8("Enchantment", () => slot.Enchantment, x => slot.Enchantment = x);
            s.EnumU8("Flags", () => slot.Flags, x => slot.Flags = x, x => ((byte)x, x.ToString()));
            s.EnumU16("Id",
                () => (int)slot.Id >= 100 ? slot.Id + 1 : slot.Id,
                x => slot.Id = (int)x > 100 ? x - 1 : x,
                x => ((ushort)x, x.ToString()));
        }

        public static Action<ISerializer> Read(Action<ItemSlot> setter) => s =>
        {
            var slot = new ItemSlot();
            Translate(slot, s);
            setter(slot.Amount == 0 ? null : slot);
        };

        public static Action<ISerializer> Write(ItemSlot slot) => s => Translate(slot, s);
    }
}