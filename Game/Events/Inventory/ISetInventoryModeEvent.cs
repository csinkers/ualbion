using UAlbion.Formats.AssetIds;
using UAlbion.Game.Gui.Inventory;

namespace UAlbion.Game.Events.Inventory
{
    public interface ISetInventoryModeEvent
    {
        InventoryMode Mode { get; }
        PartyCharacterId Member { get; }
    }
}