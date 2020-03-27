using System.Collections.Generic;

namespace UAlbion.Game.Text
{
    public class TextFormatResult
    {
        public TextFormatResult(IEnumerable<TextBlock> blocks) => Blocks = blocks;
        public IEnumerable<TextBlock> Blocks { get; }
    }
}
