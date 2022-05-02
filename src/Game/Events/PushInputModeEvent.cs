using UAlbion.Api;
using UAlbion.Api.Eventing;
using UAlbion.Formats.Config;

namespace UAlbion.Game.Events;

[Event("push_input_mode",
    "Emitted to change the currently active input mode, but allow the previous mode to be restored using pop_input_mode",
    new [] { "pim" })]
public class PushInputModeEvent : GameEvent
{
    public PushInputModeEvent(InputMode mode)
    {
        Mode = mode;
    }

    [EventPart("mode")]
    public InputMode Mode { get; }
}