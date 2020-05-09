using UAlbion.Api;
using UAlbion.Formats.Config;

namespace UAlbion.Game.Events
{
    [Event("set_mouse_mode", "Emitted to change the currently active mouse mode", new [] { "mm" })]
    public class SetMouseModeEvent : GameEvent, IHighlightEvent
    {
        public SetMouseModeEvent(MouseMode mode)
        {
            Mode = mode;
        }

        [EventPart("mode")]
        public MouseMode Mode { get; }
    }
}
