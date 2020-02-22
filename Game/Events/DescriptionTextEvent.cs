using UAlbion.Api;
using UAlbion.Game.Text;

namespace UAlbion.Game.Events
{
    [Event("description_text", "Displays some text in the description area of the status bar (lower right)")]
    public class DescriptionTextEvent : GameEvent
    {
        public DescriptionTextEvent(string text)
        {
            Text = text;
        }

        [EventPart("text", "The text to display")]
        public string Text { get; }
    }

    public class DescriptionTextExEvent : GameEvent
    {
        public DescriptionTextExEvent(ITextSource source) { Source = source; }
        public ITextSource Source { get; }
    }
}