using UAlbion.Api.Eventing;

namespace UAlbion.Game.Events;

public class StartTimerEvent : GameEvent, IVerboseEvent
{
    public StartTimerEvent(string id, float intervalSeconds, IComponent target)
    {
        Id = id;
        IntervalSeconds = intervalSeconds;
        Target = target;
    }

    public string Id { get; }
    public float IntervalSeconds { get; }
    public IComponent Target { get; }
}