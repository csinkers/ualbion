using UAlbion.Api.Eventing;

namespace UAlbion.Game.Events;

[Event("pop_input_mode", "Emitted to restore a previously active input mode")]
public class PopInputModeEvent : GameEvent { }