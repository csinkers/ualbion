using UAlbion.Api;
using UAlbion.Game.Gui.Inventory;

namespace UAlbion.Game.Events
{
    [Event("set_inv_page", "Sets the current inventory page")]
    public class SetInventoryPageEvent : GameEvent
    {
        public SetInventoryPageEvent(InventoryPage page) { Page = page; }
        [EventPart("page")] public InventoryPage Page { get; }
    }
}