using UAlbion.Api.Eventing;
using UAlbion.Formats.Assets;

namespace UAlbion.Game.Events.Inventory;

[Event("inv:pickup", "Pickup a given number of items from a party member's inventory (or all of them if amount is null)")]
public class InventoryPickupEvent : InventorySlotEvent
{
    [EventPart("amount", true)] public ushort? Amount { get; }
    public InventoryPickupEvent(ushort? amount, InventoryId sourceId, ItemSlotId slotId)
        : base(sourceId, slotId) => Amount = amount;
}