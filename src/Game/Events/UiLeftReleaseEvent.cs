using UAlbion.Api;

namespace UAlbion.Game.Events
{
    [Event("ui_left_release")]
    public class UiLeftReleaseEvent : CancellableEvent, IVerboseEvent { }
}
