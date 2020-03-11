using SerdesNet;
using UAlbion.Formats.AssetIds;

namespace UAlbion.Formats.Assets
{
    public class ItemSlot : IHoldable
    {
        public byte Amount { get; set; }
        public byte Charges { get; set; }
        public byte Enchantment { get; set; }
        public ItemSlotFlags Flags { get; set; }
        public ItemId? Id { get; set; }

        public ItemSlot DeepClone() => (ItemSlot)MemberwiseClone();
        public override string ToString() => Amount == 0 ? "Empty" : $"{Amount}x{Id} {Flags}";
        ushort IHoldable.Amount => Amount;

        public static ItemSlot Serdes(int _, ItemSlot slot, ISerializer s)  // 6 per slot
        {
            slot ??= new ItemSlot { Amount = 0, Id = ItemId.Knife };
            slot.Amount = s.UInt8(nameof(slot.Amount), slot.Amount);
            slot.Charges = s.UInt8(nameof(slot.Charges), slot.Charges);
            slot.Enchantment = s.UInt8(nameof(slot.Enchantment), slot.Enchantment);
            slot.Flags = s.EnumU8(nameof(slot.Flags), slot.Flags);
            slot.Id = (ItemId?)Tweak.Serdes(nameof(slot.Id), (ushort?)slot.Id, s.UInt16);
            return slot;
        }
    }
}
