using UAlbion.Api;
using UAlbion.Config;
using UAlbion.Formats.Assets;

namespace UAlbion.Formats.MapEvents
{
    [Event("inv:open", "Opens the given character's inventory")]
    public class InventoryOpenEvent : Event, ISetInventoryModeEvent
    {
        public InventoryOpenEvent(CharacterId member) => Submode = member;
        public InventoryMode Mode => InventoryMode.Character;
        public AssetId Submode { get; }
    }
}
