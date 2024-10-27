using UAlbion.Formats.Assets.Inv;

namespace UAlbion.Game.Events.Inventory;

public interface IInventoryEvent
{
    InventoryId Id { get; }
}