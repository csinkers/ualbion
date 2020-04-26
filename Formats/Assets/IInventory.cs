using System.Collections.Generic;

namespace UAlbion.Formats.Assets
{
    public interface IInventory
    {
        InventoryType InventoryType { get; }
        int InventoryId { get; }
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
        ItemSlot GetSlot(ItemSlotId itemSlotId);
    }
}