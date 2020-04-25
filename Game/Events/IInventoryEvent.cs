using UAlbion.Formats.AssetIds;

namespace UAlbion.Game.Events
{
    public interface IInventoryEvent
    {
        AssetType InventoryType { get; }
        int InventoryId { get; }
    }
}