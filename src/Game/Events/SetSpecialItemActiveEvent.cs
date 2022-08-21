using UAlbion.Api.Eventing;
using UAlbion.Formats.Ids;

namespace UAlbion.Game.Events;

[Event("set_special_item_active")]
public class SetSpecialItemActiveEvent : GameEvent
{
    public SetSpecialItemActiveEvent(ItemId item, bool isActive)
    {
        Item = item;
        IsActive = isActive;
    }

    [EventPart("item")] public ItemId Item { get; }
    [EventPart("active", true, true)] public bool IsActive { get; }
}