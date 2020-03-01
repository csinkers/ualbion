using System.Collections.Generic;
using UAlbion.Formats.AssetIds;

namespace UAlbion.Game.Text
{
    public class TextFormatResult
    {
        public TextFormatResult(IEnumerable<TextBlock> blocks, IList<WordId> words)
        {
            Blocks = blocks;
            Words = words;
        }

        public IEnumerable<TextBlock> Blocks { get; }
        public IList<WordId> Words { get; }
    }
}