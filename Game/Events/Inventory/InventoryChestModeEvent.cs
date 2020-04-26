using UAlbion.Api;
using UAlbion.Formats.AssetIds;
using UAlbion.Game.Gui.Inventory;

namespace UAlbion.Game.Events.Inventory
{
    [Event("inv:chest_mode", "Sets the current inventory mode")]
    public class InventoryChestModeEvent : GameEvent, ISetInventoryModeEvent
    {
        public InventoryChestModeEvent(ChestId chestId, PartyCharacterId member)
        {
            ChestId = chestId;
            Member = member;
        }

        public InventoryMode Mode => InventoryMode.Chest;
        [EventPart("chest")] public ChestId ChestId { get; }
        [EventPart("member")] public PartyCharacterId Member { get; }
    }
}
