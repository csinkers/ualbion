using System.Collections.Generic;

namespace UAlbion.Game.Text;

public class LiteralText : IText
{
    readonly TextBlock[] _blocks;
    public LiteralText(string text) => _blocks = [new TextBlock(text)];
    public LiteralText(TextBlock text) => _blocks = [text];
    public int Version => 1;
    public IEnumerable<TextBlock> GetBlocks() => _blocks;
}