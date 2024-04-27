using UAlbion.Api.Eventing;

namespace UAlbion.Game.Events;

public class TimerElapsedEvent : GameEvent, IVerboseEvent
{
    public TimerElapsedEvent(string id) { Id = id; }
    public string Id { get; }
}