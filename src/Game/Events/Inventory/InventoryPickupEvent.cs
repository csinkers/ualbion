using UAlbion.Api;
using UAlbion.Formats.Assets;

namespace UAlbion.Game.Events.Inventory
{
    [Event("inv:pickup", "Pickup a given number of items from a party member's inventory (or all of them if amount is null)")]
    public class InventoryPickupEvent : InventorySlotEvent
    {
        [EventPart("amount", true)] public ushort? Amount { get; }
        public InventoryPickupEvent(ushort? amount, InventoryType sourceType, ushort sourceId, ItemSlotId slotId)
            : base(sourceType, sourceId, slotId) => Amount = amount;
    }
}
