using UAlbion.Core.Events;
using UAlbion.Game.Input;

namespace UAlbion.Game.Events
{
    [Event("set_mouse_mode", "Emitted to change the currently active mouse mode")]
    public class SetMouseModeEvent : GameEvent
    {
        public SetMouseModeEvent(int mode)
        {
            RawMode = mode;
        }

        [EventPart("mode")]
        public int RawMode { get; }

        public MouseModeId Mode => (MouseModeId) RawMode;
    }
}