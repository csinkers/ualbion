using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Assets;
using UAlbion.Formats.MapEvents;
using UAlbion.Game.Gui.Inventory;

namespace UAlbion.Game.State.Player
{
    public interface IInventoryManager
    {
        IHoldable ItemInHand { get; }
        InventoryMode ActiveMode { get; }
        InventoryAction GetInventoryAction(InventoryType type, int id, ItemSlotId slotId);
        bool TryChangeInventory(InventoryType inventoryType, int inventoryId, ItemId itemId, QuantityChangeOperation operation, int amount);
        bool TryChangeGold(InventoryType inventoryType, int inventoryId, QuantityChangeOperation operation, int amount);
        bool TryChangeRations(InventoryType inventoryType, int inventoryId, QuantityChangeOperation operation, int amount);
    }
}