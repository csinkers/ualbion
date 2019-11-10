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
        public TextArrangement Arrangement { get; set; }

        public override string ToString() => $"[\"{Text}\" {Color} {Style} {Alignment} {Arrangement}]";

        public bool IsMergeableWith(TextBlock other)
        {
            return
                other.Text == "" ||
                other.Text == " " ||
                other.Color == Color &&
                other.Style == Style &&
                other.Alignment == Alignment &&
                other.Arrangement == Arrangement;
        }

        public void Merge(TextBlock other)
        {
            if (string.IsNullOrEmpty(other.Text))
                Text += " ";
            else
                Text += other.Text;
        }
    }
}