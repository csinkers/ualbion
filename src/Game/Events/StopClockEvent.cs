using UAlbion.Api.Eventing;

namespace UAlbion.Game.Events;

[Event("stop_clock", "Stop the game clock from advancing automatically.")]
public class StopClockEvent : GameEvent { }