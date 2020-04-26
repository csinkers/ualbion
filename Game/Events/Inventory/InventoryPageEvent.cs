using UAlbion.Api;
using UAlbion.Game.Gui.Inventory;

namespace UAlbion.Game.Events.Inventory
{
    [Event("inv:page", "Sets the current inventory page")]
    public class InventoryPageEvent : GameEvent
    {
        public InventoryPageEvent(InventoryPage page) { Page = page; }
        [EventPart("page")] public InventoryPage Page { get; }
    }
}
