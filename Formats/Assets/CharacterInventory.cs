using System.Collections.Generic;
using System.Linq;

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
        IEnumerable<ItemSlot> EnumerateAll();
        IEnumerable<ItemSlot> EnumerateBodyParts();
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

        public IEnumerable<ItemSlot> EnumerateAll()
        {
            foreach (var bodyPart in EnumerateBodyParts())
                yield return bodyPart;

            foreach (var slot in Slots.Where(x => x != null))
                yield return slot;
        }

        public IEnumerable<ItemSlot> EnumerateBodyParts()
        {
            if(Neck != null) yield return Neck;
            if(Head != null) yield return Head;
            if(Tail != null) yield return Tail;
            if(LeftHand != null) yield return LeftHand;
            if(Chest != null) yield return Chest;
            if(RightHand != null) yield return RightHand;
            if(LeftFinger != null) yield return LeftFinger;
            if(Feet != null) yield return Feet;
            if(RightFinger != null) yield return RightFinger;
        }
    }
}
