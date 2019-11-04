using UAlbion.Api;
using UAlbion.Formats.AssetIds;
using UAlbion.Game.Gui.Inventory;

namespace UAlbion.Game.Events
{
    [Event("set_inv_chest_mode", "Sets the current inventory mode")]
    public class SetInventoryChestModeEvent : GameEvent, ISetInventoryModeEvent
    {
        public SetInventoryChestModeEvent(ChestId chestId, PartyCharacterId member)
        {
            ChestId = chestId;
            Member = member;
        }

        public InventoryMode Mode => InventoryMode.Chest;
        [EventPart("chest")] public ChestId ChestId { get; }
        [EventPart("member")] public PartyCharacterId Member { get; }
    }
}