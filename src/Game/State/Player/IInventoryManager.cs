using System.Collections.Generic;
using UAlbion.Formats.Assets.Inv;
using UAlbion.Formats.Ids;

namespace UAlbion.Game.State.Player;

public interface IInventoryManager
{
    ReadOnlyItemSlot ItemInHand { get; }
    MerchantId? ActiveMerchantId { get; }
    IReadOnlyList<InventorySlotId> SellQueue { get; }
    InventoryAction GetInventoryAction(InventorySlotId id);
    int GetItemCount(InventoryId id, ItemId item);
    ushort TryGiveItems(InventoryId id, ItemSlot donor, ushort? amount); // Return the number of items that were given
    ushort TryTakeItems(InventoryId id, ItemSlot acceptor, ItemId item, ushort? amount); // Return the number of items that were taken
    bool CanEquipItem(InventoryId invId, ItemSlotId slotId, ItemId itemId);
    void ToggleSellQueue(InventorySlotId slotId);
    void SellQueued();
}