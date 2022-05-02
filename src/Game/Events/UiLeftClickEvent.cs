using UAlbion.Api;
using UAlbion.Api.Eventing;

namespace UAlbion.Game.Events;

[Event("ui_left_click")]
public class UiLeftClickEvent : CancellableEvent { }