using UAlbion.Formats.Assets;

namespace UAlbion.Game.Events.Inventory
{
    public interface IInventoryEvent
    {
        InventoryType InventoryType { get; }
        ushort InventoryId { get; }
    }
}