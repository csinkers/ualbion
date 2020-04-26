using UAlbion.Formats.Assets;

namespace UAlbion.Game.Events.Inventory
{
    public interface IInventoryEvent
    {
        InventoryType InventoryType { get; }
        int InventoryId { get; }
    }
}