using UAlbion.Api;

namespace UAlbion.Game.Events
{
    [Event("text")]
    public class TextEvent : GameEvent
    {
        public TextEvent(int textId) { TextId = textId; }
        [EventPart("textId")] public int TextId { get; }
    }
}