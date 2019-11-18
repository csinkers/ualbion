using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Assets;
using UAlbion.Game.Events;

namespace UAlbion.Game.State
{
    public class GoldInHand : IHoldable { public ushort Amount { get; set; } }
    public class RationsInHand : IHoldable { public ushort Amount { get; set; } }
    public class InventoryScreenState : IInventoryScreenState
    {
        public IHoldable ItemInHand { get; set; }
        public InventoryPickupItemEvent ReturnItemInHandEvent { get; set; }
        public PartyCharacterId ActiveCharacterId { get; set; }
    }
    public interface IInventoryScreenState
    {
        IHoldable ItemInHand { get; }
        InventoryPickupItemEvent ReturnItemInHandEvent { get; }
        PartyCharacterId ActiveCharacterId { get; }
    }

}