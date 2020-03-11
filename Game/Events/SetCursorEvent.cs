using UAlbion.Api;
using UAlbion.Formats.AssetIds;

namespace UAlbion.Game.Events
{
    [Event("set_cursor", "Change the currently active mouse cursor")]
    public class SetCursorEvent : GameEvent
    {
        public SetCursorEvent(CoreSpriteId cursorId) { CursorId = cursorId; }
        [EventPart("cursor_id")] public CoreSpriteId CursorId { get; }
    }
}
