using UAlbion.Api;
using UAlbion.Formats.Config;

namespace UAlbion.Game.Events;

[Event("input_mode", "Emitted to change the currently active input mode", "im")]
public class InputModeEvent : GameEvent
{
    public InputModeEvent(InputMode? mode) => Mode = mode;

    [EventPart("mode", true)]
    public InputMode? Mode { get; }
}