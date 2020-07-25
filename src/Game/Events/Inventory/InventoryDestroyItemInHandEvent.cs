using UAlbion.Api;

namespace UAlbion.Game.Events.Inventory
{
    [Event("inv:destroy_item_in_hand", "Destroy the currently held item, if any")]
    public class InventoryDestroyItemInHandEvent : GameEvent { }
}