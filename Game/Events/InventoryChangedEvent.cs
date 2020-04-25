using UAlbion.Api;
using UAlbion.Formats.AssetIds;

namespace UAlbion.Game.Events
{
    public class InventoryChangedEvent : GameEvent, IVerboseEvent, IInventoryEvent
    {
        public InventoryChangedEvent(AssetType type, int id)
        {
            InventoryType = type;
            InventoryId = id;
        }

        public AssetType InventoryType { get; }
        public int InventoryId { get; }
    }
}
