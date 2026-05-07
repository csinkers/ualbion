using UAlbion.Api.Eventing;
using UAlbion.Formats.Assets.Inv;

namespace UAlbion.Game.Events.Inventory;

[Event("inv:updated")]
public class InventoryChangedEvent : GameEvent, IInventoryEvent, IVerboseEvent
{
    public InventoryChangedEvent(InventoryId id) => Id = id;
    [EventPart("id")] public InventoryId Id { get; }

    /// <summary>False when this is only a lerp/UI animation tick, not an actual inventory mutation.</summary>
    public bool IsRealChange { get; init; } = true;
}
