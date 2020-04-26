using UAlbion.Api;
using UAlbion.Formats.AssetIds;
using UAlbion.Game.Gui.Inventory;

namespace UAlbion.Game.Events.Inventory
{
    [Event("inv:char_mode", "Sets the current inventory mode (provided the inventory is the currently active scene)")]
    public class InventoryModeEvent : GameEvent, ISetInventoryModeEvent
    {
        public InventoryModeEvent(PartyCharacterId member)
        {
            Member = member;
        }

        public InventoryMode Mode => InventoryMode.Character;
        [EventPart("member")] public PartyCharacterId Member { get; }
    }
}
