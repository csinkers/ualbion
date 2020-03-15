using UAlbion.Core;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Assets;
using UAlbion.Game.Events;

namespace UAlbion.Game.State
{
    public interface IInventoryScreenState
    {
        IHoldable ItemInHand { get; }
        InventoryPickupItemEvent ReturnItemInHandEvent { get; }
    }

    public class GoldInHand : IHoldable { public ushort Amount { get; set; } }
    public class RationsInHand : IHoldable { public ushort Amount { get; set; } }

    public class InventoryScreenState : ServiceComponent<IInventoryScreenState>, IInventoryScreenState
    {
        static readonly HandlerSet Handlers = new HandlerSet(
            H<InventoryScreenState, ClearInventoryItemInHandEvent>((x, e) =>
            {
                x.ItemInHand = null;
                x.ReturnItemInHandEvent = null;
            }),
            H<InventoryScreenState, SetInventoryItemInHandEvent>((x, e) =>
            {
                x.ItemInHand = e.ItemInHand;
                x.ReturnItemInHandEvent = new InventoryPickupItemEvent(e.Id, e.SlotId);
            })
        );

        public InventoryScreenState() : base(Handlers) { }

        public IHoldable ItemInHand { get; private set; }
        public InventoryPickupItemEvent ReturnItemInHandEvent { get; private set; }
    }

    public class ClearInventoryItemInHandEvent : GameEvent { }
    public class SetInventoryItemInHandEvent : GameEvent
    {
        public SetInventoryItemInHandEvent(IHoldable itemInHand, PartyCharacterId id, ItemSlotId slotId)
        {
            ItemInHand = itemInHand;
            Id = id;
            SlotId = slotId;
        }

        public IHoldable ItemInHand { get; }
        public PartyCharacterId Id { get; }
        public ItemSlotId SlotId { get; }
    }
}
