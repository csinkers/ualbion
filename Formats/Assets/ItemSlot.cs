using UAlbion.Formats.AssetIds;

namespace UAlbion.Formats.Assets
{
    public class ItemSlot
    {
        public byte Amount { get; set; }
        public byte Charges { get; set; }
        public byte Enchantment { get; set; }
        public ItemSlotFlags Flags { get; set; }
        public ItemId Id { get; set; }

        public override string ToString() => Amount == 0 ? "Empty" : $"{Amount}x{Id} {Flags}";
    }
}