using UAlbion.Api;
using UAlbion.Config;

namespace UAlbion.Formats.MapEvents
{
    [Event("inv:open", "Opens the given character's inventory")]
    public class InventoryOpenEvent : Event, ISetInventoryModeEvent
    {
        public InventoryOpenEvent(AssetId member) => Submode = member;
        public InventoryMode Mode => InventoryMode.Character;
        [EventPart("member")] public AssetId Submode { get; }
    }
}
