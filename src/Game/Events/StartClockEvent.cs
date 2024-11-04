using UAlbion.Api.Eventing;

namespace UAlbion.Game.Events;

[Event("start_clock", "Resume automatically updating the game clock.")]
public class StartClockEvent : GameEvent
{
    public static StartClockEvent Instance { get; } = new();
}