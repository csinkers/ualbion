using UAlbion.Api;
using UAlbion.Formats.Config;

namespace UAlbion.Game.Events
{
    [Event("push_input_mode", "Emitted to change the currently active input mode, but allow the previous mode to be restored using pop_input_mode")]
    public class PushInputModeEvent : GameEvent
    {
        public PushInputModeEvent(int mode)
        {
            RawMode = mode;
        }

        [EventPart("mode")]
        public int RawMode { get; }

        public InputMode Mode => (InputMode) RawMode;
    }
}