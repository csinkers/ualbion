using UAlbion.Api.Eventing;

namespace UAlbion.Game.Events;

public class WallClockTimerEvent : GameEvent, IVerboseEvent
{
    public WallClockTimerEvent(float intervalSeconds) => IntervalSeconds = intervalSeconds;
    public float IntervalSeconds { get; }
}