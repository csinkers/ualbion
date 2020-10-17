using UAlbion.Formats.Assets;

namespace UAlbion.Game.Events.Inventory
{
    public interface IInventoryEvent
    {
        InventoryId Id { get; }
    }
}