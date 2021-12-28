using UAlbion.Formats.Assets;

namespace UAlbion.Game.State.Player;

public interface IInventoryManager
{
    ReadOnlyItemSlot ItemInHand { get; }
    InventoryAction GetInventoryAction(InventorySlotId id);
    int GetItemCount(InventoryId id, ItemId item);
    ushort TryGiveItems(InventoryId id, ItemSlot donor, ushort? amount); // Return the number of items that were given
    ushort TryTakeItems(InventoryId id, ItemSlot acceptor, ItemId item, ushort? amount); // Return the number of items that were taken
}