using UAlbion.Api;
using UAlbion.Formats.AssetIds;
using UAlbion.Game.Gui.Inventory;

namespace UAlbion.Game.Events
{
    public interface ISetInventoryModeEvent
    {
        InventoryMode Mode { get; }
        PartyCharacterId Member { get; }
    }

    [Event("set_inv_mode", "Sets the current inventory mode (provided the inventory is the currently active scene)")]
    public class SetInventoryModeEvent : GameEvent, ISetInventoryModeEvent
    {
        public SetInventoryModeEvent(PartyCharacterId member)
        {
            Member = member;
        }

        public InventoryMode Mode => InventoryMode.Character;
        [EventPart("member")] public PartyCharacterId Member { get; }
    }
}