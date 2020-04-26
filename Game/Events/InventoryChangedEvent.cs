using UAlbion.Api;
using UAlbion.Formats.Assets;

namespace UAlbion.Game.Events
{
    public class InventoryChangedEvent : GameEvent, IVerboseEvent, IInventoryEvent
    {
        public InventoryChangedEvent(InventoryType type, int id)
        {
            InventoryType = type;
            InventoryId = id;
        }

        public InventoryType InventoryType { get; }
        public int InventoryId { get; }
    }
}
