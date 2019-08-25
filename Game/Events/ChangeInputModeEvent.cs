using UAlbion.Formats.Config;

namespace UAlbion.Game.Events
{
    //[MapEvent("change_input_mode", "Changes the currently active input mode / keybindings")]
    class ChangeInputModeEvent : GameEvent
    {
        //[EventPart("mode")]
        public InputMode Mode { get; }
        public ChangeInputModeEvent(InputMode mode)
        {
            Mode = mode;
        }
    }
}