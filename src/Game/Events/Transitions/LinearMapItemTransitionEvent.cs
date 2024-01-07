using UAlbion.Api.Eventing;
using UAlbion.Formats.Ids;

namespace UAlbion.Game.Events.Transitions;

[Event("linear_map_item_transition")]
public class LinearMapItemTransitionEvent : Event
{
    public LinearMapItemTransitionEvent(ItemId itemId, int x, int y, float? transitionTime)
    {
        ItemId = itemId;
        X = x;
        Y = y;
        TransitionTime = transitionTime;
    }

    [EventPart("item_id")] public ItemId ItemId { get; }
    [EventPart("x")] public int X { get; }
    [EventPart("y")] public int Y { get; }
    [EventPart("time_ms", true)] public float? TransitionTime { get; }
}