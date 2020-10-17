using UAlbion.Api;
using UAlbion.Formats.Assets;

namespace UAlbion.Game.Events.Inventory
{
    [Event("inv:refresh")]
    public class InventoryChangedEvent : GameEvent, IInventoryEvent, IVerboseEvent
    {
        public InventoryChangedEvent(InventoryId id) => Id = id;
        [EventPart("id")] public InventoryId Id { get; }
    }
}
