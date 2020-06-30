using UAlbion.Api;
using UAlbion.Formats.AssetIds;

namespace UAlbion.Game.Events.Inventory
{
    [Event("inv:give", "Give the currently held item(s) to another party member.")]
    public class InventoryGiveItemEvent : GameEvent, IAsyncEvent
    {
        public InventoryGiveItemEvent(PartyCharacterId memberId) { MemberId = memberId; }
        [EventPart("memberId", "The party member to give to.")] public PartyCharacterId MemberId { get; }
    }
}
