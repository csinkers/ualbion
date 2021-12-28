using UAlbion.Api;

namespace UAlbion.Game.Events.Inventory;

[Event("inv:return_item_in_hand", "Return the currently held item to where it was picked up from")]
public class InventoryReturnItemInHandEvent : GameEvent { }