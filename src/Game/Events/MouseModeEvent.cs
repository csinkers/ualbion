using UAlbion.Api;
using UAlbion.Formats.Config;

namespace UAlbion.Game.Events
{
    [Event("mouse_mode", "Emitted to change the currently active mouse mode", new [] { "mm" })]
    public class MouseModeEvent : GameEvent
    {
        public MouseModeEvent(MouseMode? mode)
        {
            Mode = mode;
        }

        [EventPart("mode", true)]
        public MouseMode? Mode { get; }
    }
}
