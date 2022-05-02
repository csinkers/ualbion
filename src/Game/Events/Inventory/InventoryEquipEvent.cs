using UAlbion.Api;
using UAlbion.Api.Eventing;

namespace UAlbion.Game.Events.Inventory;

[Event("inv:equip", "Equip the item currently beneath the mouse cursor (if possible)")]
public class InventoryEquipEvent { }