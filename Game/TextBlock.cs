using UAlbion.Formats.AssetIds;

namespace UAlbion.Game
{
    public class TextBlock
    {
        public TextBlock() : this(string.Empty) { }
        public TextBlock(string text)
        {
            Text = text;
            Color = FontColor.White;
        }

        public string Text { get; set; }
        public FontColor Color { get; set; }
        public TextStyle Style { get; set; }
        public TextAlignment Alignment { get; set; }
        public bool ForceLineBreak { get; set; }
    }
}