using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Assets;

namespace UAlbion.Formats.Parsers
{
    static class ItemSlotLoader
    {
        public static ItemSlot Serdes(int _, ItemSlot slot, ISerializer s)  // 6 per slot
        {
            slot ??= new ItemSlot { Amount = 0, Id = ItemId.Knife };
            slot.Amount = s.UInt8(nameof(slot.Amount), slot.Amount);
            slot.Charges = s.UInt8(nameof(slot.Charges), slot.Charges);
            slot.Enchantment = s.UInt8(nameof(slot.Enchantment), slot.Enchantment);
            slot.Flags = s.EnumU8(nameof(slot.Flags), slot.Flags);
            slot.Id = (ItemId)(Tweak.Serdes(nameof(slot.Id), (ushort?)slot.Id, s.UInt16) ?? 0);
            return slot;
        }
    }
}