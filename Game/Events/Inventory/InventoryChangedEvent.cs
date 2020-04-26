using UAlbion.Api;
using UAlbion.Formats.Assets;

namespace UAlbion.Game.Events.Inventory
{
    [Event("inv:refresh")]
    public class InventoryChangedEvent : GameEvent, IInventoryEvent, IVerboseEvent
    {
        public InventoryChangedEvent(InventoryType type, int id)
        {
            InventoryType = type;
            InventoryId = id;
        }

        [EventPart("type")] public InventoryType InventoryType { get; }
        [EventPart("id")] public int InventoryId { get; }
    }
}
