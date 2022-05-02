using UAlbion.Api.Eventing;

namespace UAlbion.Game.Events;

[Event("inv:open_pos", "Opens the inventory screen for the character in the given status bar position")]
public class InventoryOpenPositionEvent : GameEvent
{
    public InventoryOpenPositionEvent(int position) => Position = position;
    [EventPart("position")] public int Position { get; }
}