using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Assets;
using UAlbion.Formats.MapEvents;

namespace UAlbion.Game.State.Player
{
    public interface IInventoryManager
    {
        IHoldable ItemInHand { get; }
        InventoryAction GetInventoryAction(AssetType type, int id, ItemSlotId slotId);
        bool TryChangeInventory(AssetType inventoryType, int inventoryId, ItemId itemId, QuantityChangeOperation operation, int amount, EventContext context);
        bool TryChangeGold(AssetType inventoryType, int inventoryId, QuantityChangeOperation operation, int amount, EventContext context);
        bool TryChangeRations(AssetType inventoryType, int inventoryId, QuantityChangeOperation operation, int amount, EventContext context);
    }
}