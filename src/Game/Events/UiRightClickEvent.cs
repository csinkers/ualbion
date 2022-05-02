using UAlbion.Api;
using UAlbion.Api.Eventing;

namespace UAlbion.Game.Events;

[Event("ui_right_click")]
public class UiRightClickEvent : CancellableEvent { }