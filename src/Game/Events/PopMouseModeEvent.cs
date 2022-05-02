using UAlbion.Api;
using UAlbion.Api.Eventing;

namespace UAlbion.Game.Events;

[Event("pop_mouse_mode", "Emitted to restore a previously active mouse mode")]
public class PopMouseModeEvent : GameEvent { }