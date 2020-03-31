using UAlbion.Api;
using UAlbion.Game.Text;

namespace UAlbion.Game.Events
{
    [Event("hover_text", "Displays some text in the hover / status area of the status bar (upper right)")]
    public class HoverTextEvent : GameEvent, IVerboseEvent
    {
        public HoverTextEvent(string text)
        {
            Text = text;
        }

        [EventPart("text", "The text to display")]
        public string Text { get; }
    }

    public class HoverTextExEvent : GameEvent
    {
        public HoverTextExEvent(IText source) { Source = source; }
        public IText Source { get; }
    }
}
