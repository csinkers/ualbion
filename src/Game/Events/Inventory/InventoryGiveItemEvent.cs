using UAlbion.Api;
using UAlbion.Formats.Assets;

namespace UAlbion.Game.Events.Inventory;

[Event("inv:give", "Give the currently held item(s) to another party member.")]
public class InventoryGiveItemEvent : GameEvent, IAsyncEvent
{
    public InventoryGiveItemEvent(PartyMemberId memberId) { MemberId = memberId; }
    [EventPart("memberId", "The party member to give to.")] public PartyMemberId MemberId { get; }
}