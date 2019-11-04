using System.Collections.Generic;

namespace UAlbion.Game
{
    public class LiteralText : ITextSource
    {
        readonly TextBlock[] _blocks;
        public LiteralText(string text)
        {
            _blocks = new[] { new TextBlock(text) };
        }

        public int Version => 1;
        public IEnumerable<TextBlock> Get() => _blocks;
    }
}