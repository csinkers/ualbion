using UAlbion.Formats.Assets;

namespace UAlbion.Game.Events
{
    public interface IInventoryEvent
    {
        InventoryType InventoryType { get; }
        int InventoryId { get; }
    }
}