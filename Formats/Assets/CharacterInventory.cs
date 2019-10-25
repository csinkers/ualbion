namespace UAlbion.Formats.Assets
{
    public interface ICharacterInventory
    {
        ushort Gold { get; }
        ushort Rations { get; }
        ItemSlot Neck { get; }
        ItemSlot Head { get; }
        ItemSlot Tail { get; }
        ItemSlot LeftHand { get; }
        ItemSlot Chest { get; }
        ItemSlot RightHand { get; }
        ItemSlot LeftFinger { get; }
        ItemSlot Feet { get; }
        ItemSlot RightFinger { get; }
        ItemSlot[] Slots { get; }
    }
    public class CharacterInventory : ICharacterInventory
    {
        public ushort Gold { get; set; }
        public ushort Rations { get; set; }
        public ItemSlot Neck { get; set; }
        public ItemSlot Head { get; set; }
        public ItemSlot Tail { get; set; }
        public ItemSlot LeftHand { get; set; }
        public ItemSlot Chest { get; set; }
        public ItemSlot RightHand { get; set; }
        public ItemSlot LeftFinger { get; set; }
        public ItemSlot Feet { get; set; }
        public ItemSlot RightFinger { get; set; }
        public ItemSlot[] Slots { get; set; } // 24
    }
}