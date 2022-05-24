using UAlbion.Api.Eventing;
using UAlbion.Formats.Ids;

namespace UAlbion.Game.Events;

[Event("activate_item")]
public class ActivateItemEvent : GameEvent
{
    public ActivateItemEvent(ItemId item) => Item = item;
    [EventPart("item")] public ItemId Item { get; }
}