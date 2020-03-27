using System.Collections.Generic;
using System.Linq;
using UAlbion.Formats.AssetIds;

namespace UAlbion.Game.Text
{
    public class TextBlock // Logical segment of text where all glyphs share the same formatting.
    {
        public TextBlock() : this(0, string.Empty) { }
        public TextBlock(int blockId) : this(blockId, string.Empty) { }
        public TextBlock(string text) : this(0, text) { }
        public TextBlock(int blockId, string text)
        {
            BlockId = blockId;
            Text = text ?? "";
            Color = FontColor.White;
        }

        string _text;
#if DEBUG
        public string Raw { get; set; }
#endif
        public int BlockId { get; }
        public string Text { get => _text; set => _text = value ?? ""; }
        public FontColor Color { get; set; }
        public TextStyle Style { get; set; }
        public TextAlignment Alignment { get; set; }
        public TextArrangement Arrangement { get; set; }
        public IEnumerable<WordId> Words => _words ?? Enumerable.Empty<WordId>();
        public void AddWord(WordId word) { _words ??= new HashSet<WordId>(); _words.Add(word); }
        ISet<WordId> _words;

        public override string ToString() => $"[\"{Text}\" {Color} {Style} {Alignment} {Arrangement}]";

        public bool IsMergeableWith(TextBlock other)
        {
            return
                other.BlockId == BlockId &&
                (other.Text == "" ||
                other.Text == " " ||
                other.Color == Color &&
                other.Style == Style &&
                other.Alignment == Alignment &&
                other.Arrangement == Arrangement);
        }

        public void Merge(TextBlock other)
        {
            if (string.IsNullOrEmpty(other.Text))
                Text += " ";
            else
                Text += other.Text;

            if (other._words != null)
            {
                _words ??= new HashSet<WordId>();
                _words.UnionWith(other._words);
            }
        }
    }
}
