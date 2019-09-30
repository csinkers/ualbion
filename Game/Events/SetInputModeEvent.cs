using UAlbion.Api;
using UAlbion.Formats.Config;

namespace UAlbion.Game.Events
{
    [Event("set_input_mode", "Emitted to change the currently active input mode")]
    public class SetInputModeEvent : GameEvent
    {
        public SetInputModeEvent(InputMode mode)
        {
            Mode = mode;
        }

        [EventPart("mode")]
        public InputMode Mode { get; }
    }
}