using UAlbion.Api.Eventing;
using UAlbion.Formats.Assets;

namespace UAlbion.Game.Events.Inventory;

[Event("inv:updated")]
public class InventoryChangedEvent : GameEvent, IInventoryEvent, IVerboseEvent
{
    public InventoryChangedEvent(InventoryId id) => Id = id;
    [EventPart("id")] public InventoryId Id { get; }
}