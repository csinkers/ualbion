using UAlbion.Api;
using UAlbion.Formats.Assets;

namespace UAlbion.Game.Events.Inventory
{
    [Event("inv:take_all", "Take everything from the given chest")]
    public class InventoryTakeAllEvent : GameEvent
    {
        public InventoryTakeAllEvent(ChestId chestId) => ChestId = chestId;
        [EventPart("chest_id", "The id of the chest to take from")] public ChestId ChestId { get; }
    }
}
