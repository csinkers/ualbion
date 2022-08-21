using UAlbion.Api.Eventing;
using UAlbion.Game.Gui.Inventory;

namespace UAlbion.Game.Events.Inventory;

[Event("inv:set_page")]
public class InventorySetPageEvent : GameEvent
{
    public InventorySetPageEvent(InventoryPage page) => Page = page;
    [EventPart("page")] public InventoryPage Page { get; }
}