using UAlbion.Api.Eventing;
using UAlbion.Formats.Assets.Inv;

namespace UAlbion.Game.Events.Inventory;

[Event("inv:discard")]
public class InventoryDiscardEvent : InventorySlotEvent
{
    [EventPart("norm_x")] public float NormX { get; }
    [EventPart("norm_y")] public float NormY { get; }

    public InventoryDiscardEvent(float normX, float normY, InventoryId inventoryId, ItemSlotId slotId)
        : base(inventoryId, slotId)
    {
        NormX = normX;
        NormY = normY;
    }
}