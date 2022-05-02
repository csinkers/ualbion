using UAlbion.Api;
using UAlbion.Api.Eventing;

namespace UAlbion.Game.Events;

[Event("close_window", "Closes the currently active window")]
public class CloseWindowEvent : CancellableEvent { }