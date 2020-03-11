using UAlbion.Api;
using UAlbion.Formats.AssetIds;

namespace UAlbion.Game.Events
{
    [Event("inv:give", "Give the currently held item(s) to another party member.")]
    public class InventoryGiveItemEvent : GameEvent, IPartyEvent
    {
        public InventoryGiveItemEvent(PartyCharacterId memberId) { MemberId = memberId; }
        [EventPart("member", "The party member to give to.")] public PartyCharacterId MemberId { get; }
    }
}
