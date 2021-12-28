using UAlbion.Api;
using UAlbion.Formats.Assets;

namespace UAlbion.Game.Events;

[Event("set_cursor", "Change the currently active mouse cursor")]
public class SetCursorEvent : GameEvent, IVerboseEvent
{
    public SetCursorEvent(SpriteId cursorId) => CursorId = cursorId;
    [EventPart("cursor_id")] public SpriteId CursorId { get; }
}