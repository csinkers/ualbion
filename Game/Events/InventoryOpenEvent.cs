using UAlbion.Api;
using UAlbion.Formats.AssetIds;

namespace UAlbion.Game.Events
{
    [Event("inv:open", "Opens the inventory screen for the given character")]
    public class InventoryOpenEvent : GameEvent
    {
        public InventoryOpenEvent(PartyCharacterId memberId) => MemberId = memberId;
        [EventPart("memberId")] public PartyCharacterId MemberId { get; }
    }
}
