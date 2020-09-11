using UAlbion.Api;
using UAlbion.Formats.AssetIds;

namespace UAlbion.Formats.MapEvents
{
    [Event("inv:open", "Opens the given character's inventory")]
    public class InventoryOpenEvent : Event, ISetInventoryModeEvent
    {
        public InventoryOpenEvent(PartyCharacterId? member) => Member = member;
        [EventPart("member", true)] public PartyCharacterId? Member { get; }
        public InventoryMode Mode => InventoryMode.Character;
        public ushort Submode => 0;
    }
}
