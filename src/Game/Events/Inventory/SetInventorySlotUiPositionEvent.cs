using UAlbion.Api;
using UAlbion.Formats.Assets;

namespace UAlbion.Game.Events.Inventory;

public class SetInventorySlotUiPositionEvent : GameEvent, IVerboseEvent
{
    public SetInventorySlotUiPositionEvent(InventorySlotId id, int x, int y)
    {
        InventorySlotId = id;
        X = x;
        Y = y;
    }

    public InventorySlotId InventorySlotId { get; }
    public int X { get; }
    public int Y { get; }
}