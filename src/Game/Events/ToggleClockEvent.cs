using UAlbion.Api.Eventing;

namespace UAlbion.Game.Events;

[Event("toggle_clock", "Toggle the game clock between the stopped and running states")]
public class ToggleClockEvent : GameEvent { }