using UAlbion.Formats.Assets;
using UAlbion.Formats.MapEvents;
using UAlbion.Game.Gui.Inventory;

namespace UAlbion.Game.State.Player
{
    public interface IInventoryManager
    {
        ReadOnlyItemSlot ItemInHand { get; }
        InventoryMode ActiveMode { get; }
        InventoryAction GetInventoryAction(InventorySlotId id);
        bool TryChangeInventory(InventoryId id, IContents contents, QuantityChangeOperation operation, int amount);
    }
}