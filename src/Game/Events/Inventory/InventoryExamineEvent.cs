using UAlbion.Api;
using UAlbion.Formats.Assets;

namespace UAlbion.Game.Events.Inventory
{
    [Event("inv:examine")]
    public class InventoryExamineEvent : GameEvent
    {
        public InventoryExamineEvent(ItemId id) => ItemId = id;
        [EventPart("item_id", "The item to examine")] public ItemId ItemId { get; }
    }
}
