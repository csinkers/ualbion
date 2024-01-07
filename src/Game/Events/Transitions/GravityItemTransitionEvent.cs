using UAlbion.Api.Eventing;
using UAlbion.Formats.Ids;

namespace UAlbion.Game.Events.Transitions;

[Event("gravity_item_transition")]
public class GravityItemTransitionEvent : Event
{
    public GravityItemTransitionEvent(ItemId itemId, float fromNormX, float fromNormY)
    {
        ItemId = itemId;
        FromNormX = fromNormX;
        FromNormY = fromNormY;
    }

    [EventPart("item_id")] public ItemId ItemId { get; }
    [EventPart("from_x")] public float FromNormX { get; }
    [EventPart("from_y")] public float FromNormY { get; }
}