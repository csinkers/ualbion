using System;
using System.Collections.Generic;

namespace UAlbion.Game.Text;

public class TextFallback : IText
{
    readonly IText[] _texts;
    public TextFallback(params IText[] texts)
    {
        _texts = texts ?? throw new ArgumentNullException(nameof(texts));
        if (texts.Length < 2)
            throw new ArgumentException("Must supply at least two arguments to TextFallback");
    }

    public int Version => _texts[0].Version;
    public IEnumerable<TextBlock> GetBlocks()
    {
        bool done = false;
        foreach (var text in _texts)
        {
            foreach (var block in text.GetBlocks())
            {
                yield return block;
                done = true;
            }

            if (done)
                break;
        }
    }
}