using UAlbion.Formats.AssetIds;

namespace UAlbion.Game.Events
{
    public class SetCursorEvent : GameEvent
    {
        public SetCursorEvent(CoreSpriteId cursorId)
        {
            CursorId = cursorId;
        }

        public CoreSpriteId CursorId { get; }
    }
}