using UAlbion.Api.Eventing;
using UAlbion.Formats.Ids;
using UAlbion.Game.Gui.Inventory;

namespace UAlbion.Game.Events;

[Event("inv:open_pos", "Opens the inventory screen for the character in the given status bar position")]
public class InventoryOpenPositionEvent : GameEvent
{
    public InventoryOpenPositionEvent(int position) => Position = position;
    [EventPart("position")] public int Position { get; }
}

[Event("inv:set_page")]
public class InventorySetPageEvent : GameEvent
{
    public InventorySetPageEvent(InventoryPage page) => Page = page;
    [EventPart("page")] public InventoryPage Page { get; }
}

[Event("inv:open", "Opens the given character's inventory")]
public class InventoryOpenEvent : Event
{
    public InventoryOpenEvent(PartyMemberId member) => PartyMemberId = member;
    [EventPart("member")] public PartyMemberId PartyMemberId { get; }
}