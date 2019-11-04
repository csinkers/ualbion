using System.Collections.Generic;
using System.Linq;
using UAlbion.Formats.AssetIds;

namespace UAlbion.Game
{
    public class TextBlock
    {
        public TextBlock() : this(string.Empty, Enumerable.Empty<WordId>()) { }
        public TextBlock(IEnumerable<WordId> words) : this(string.Empty, words) { }
        public TextBlock(string text) : this(text, Enumerable.Empty<WordId>()) { }
        public TextBlock(string text, IEnumerable<WordId> words)
        {
            Text = text;
            Color = FontColor.White;
            Words = words.ToList();
        }

        public string Text { get; set; }
        public FontColor Color { get; set; }
        public IList<WordId> Words { get; }
        public TextStyle Style { get; set; }
        public TextAlignment Alignment { get; set; }
        public bool ForceLineBreak { get; set; }
    }
}