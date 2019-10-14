using UAlbion.Api;
using UAlbion.Formats.Config;

namespace UAlbion.Game.Events
{
    [Event("push_mouse_mode", "Emitted to change the currently active mouse mode, but allow the previous mode to be restored using pop_mouse_mode")]
    public class PushMouseModeEvent : GameEvent
    {
        public PushMouseModeEvent(MouseMode mode)
        {
            Mode = mode;
        }

        [EventPart("mode")]
        public MouseMode Mode { get; }
    }
}