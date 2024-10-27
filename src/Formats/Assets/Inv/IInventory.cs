using System.Collections.Generic;

namespace UAlbion.Formats.Assets.Inv;

public interface IInventory
{
    InventoryId Id { get; }
    IReadOnlyItemSlot Gold { get; }
    IReadOnlyItemSlot Rations { get; }
    IReadOnlyItemSlot Neck { get; }
    IReadOnlyItemSlot Head { get; }
    IReadOnlyItemSlot Tail { get; }
    IReadOnlyItemSlot LeftHand { get; }
    IReadOnlyItemSlot Chest { get; }
    IReadOnlyItemSlot RightHand { get; }
    IReadOnlyItemSlot LeftFinger { get; }
    IReadOnlyItemSlot Feet { get; }
    IReadOnlyItemSlot RightFinger { get; }
    IReadOnlyList<IReadOnlyItemSlot> Slots { get; }
    IEnumerable<IReadOnlyItemSlot> EnumerateAll();
    IEnumerable<IReadOnlyItemSlot> EnumerateBodyParts();
    IReadOnlyItemSlot GetSlot(ItemSlotId itemSlotId);
    bool IsEmpty { get; }
}