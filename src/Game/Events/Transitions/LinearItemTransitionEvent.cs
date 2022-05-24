using System.Numerics;
using UAlbion.Api.Eventing;
using UAlbion.Formats.Ids;

namespace UAlbion.Game.Events.Transitions;

[Event("linear_item_transition")]
public class LinearItemTransitionEvent : Event, IAsyncEvent
{
    public LinearItemTransitionEvent(ItemId itemId, int fromX, int fromY, int toX, int toY, float? transitionTime)
    {
        ItemId = itemId;
        FromX = fromX;
        FromY = fromY;
        ToX = toX;
        ToY = toY;
        TransitionTime = transitionTime;
    }

    [EventPart("item_id")] public ItemId ItemId { get; }
    [EventPart("from_x")] public int FromX { get; } // UI coordinates
    [EventPart("from_y")] public int FromY { get; }
    [EventPart("to_x")] public int ToX { get; } // UI coordinates
    [EventPart("to_y")] public int ToY { get; }
    [EventPart("time_ms", true)] public float? TransitionTime { get; }
    public Vector2 FromUiPosition => new(FromX, FromY);
    public Vector2 ToUiPosition => new(ToX, ToY);
}