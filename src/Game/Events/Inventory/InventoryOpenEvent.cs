using UAlbion.Api.Eventing;
using UAlbion.Formats.Ids;

namespace UAlbion.Game.Events.Inventory;

[Event("inv:open", "Opens the given character's inventory")]
public class InventoryOpenEvent : Event
{
    public InventoryOpenEvent(PartyMemberId member) => PartyMemberId = member;
    [EventPart("member")] public PartyMemberId PartyMemberId { get; }
}