using UAlbion.Formats.AssetIds;

namespace UAlbion.Formats.Assets
{
    public interface IHoldable
    {
        ushort Amount { get; }
    }

    public class ItemSlot : IHoldable
    {
        public byte Amount { get; set; }
        public byte Charges { get; set; }
        public byte Enchantment { get; set; }
        public ItemSlotFlags Flags { get; set; }
        public ItemId Id { get; set; }

        public ItemSlot DeepClone() => (ItemSlot)MemberwiseClone();
        public override string ToString() => Amount == 0 ? "Empty" : $"{Amount}x{Id} {Flags}";
        ushort IHoldable.Amount => Amount;
    }
}