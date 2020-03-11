using UAlbion.Api;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Assets;

namespace UAlbion.Game.Events
{
    [Event("inv:pickup", "Pickup one or more items from a party member's equipped items or inventory and/or drop the currently held item(s)'")]
    public class InventoryPickupItemEvent : GameEvent, IPartyEvent
    {
        public InventoryPickupItemEvent(PartyCharacterId memberId, ItemSlotId slotId, int? quantity = null)
        {
            MemberId = memberId;
            SlotId = slotId;
            Quantity = quantity;
        }

        [EventPart("member", "The party member to take from / give to.")] public PartyCharacterId MemberId { get; }
        [EventPart("slot", "The body or inventory slot to take from / give to. Defaults to the first empty inventory slot if not supplied.")] public ItemSlotId SlotId { get; }
        [EventPart("quantity", "The number of items in the slot to pick up. Defaults to all items if not supplied.", true)] public int? Quantity { get; }
    }
}
