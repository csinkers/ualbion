using System;
using System.Collections.Generic;
using System.Linq;
using UAlbion.Formats.Assets;

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

        public string Raw { get; set; }
        public int BlockId { get; }
        public string Text { get => _text; set => _text = value ?? ""; }
        public FontColor Color { get; set; }
        public TextStyle Style { get; set; }
        public TextAlignment Alignment { get; set; }
        public TextArrangementFlags ArrangementFlags { get; set; }
        public IEnumerable<WordId> Words => _words ?? Enumerable.Empty<WordId>();
        public void AddWord(WordId word) { _words ??= new HashSet<WordId>(); _words.Add(word); }
        ISet<WordId> _words;

        public override string ToString() => $"[\"{Text}\" {Color} {Style} {Alignment} {ArrangementFlags}]";

        public bool IsMergeableWith(TextBlock other)
        {
            if (other == null) throw new ArgumentNullException(nameof(other));
            return
                other.BlockId == BlockId &&
                (string.IsNullOrWhiteSpace(other.Text) ||
                other.Color == Color &&
                other.Style == Style &&
                other.Alignment == Alignment &&
                other.ArrangementFlags == ArrangementFlags);
        }

        public void Merge(TextBlock other)
        {
            if (other == null) throw new ArgumentNullException(nameof(other));
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
