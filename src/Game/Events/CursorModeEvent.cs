using UAlbion.Api.Eventing;
using UAlbion.Game.Input;

namespace UAlbion.Game.Events;

[Event("cursor_mode")]
public class CursorModeEvent : GameEvent, IVerboseEvent
{
    public CursorModeEvent(CursorMode mode) => Mode = mode;

    [EventPart("mode")]
    public CursorMode Mode { get; }
}