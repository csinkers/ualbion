using System;
using System.Collections.Generic;

namespace UAlbion.Game.Text;

public class TextFilter : IText
{
    readonly Func<TextBlock, bool> _filter;
    IText _source;
    int _baseVersion;

    public TextFilter(Func<TextBlock, bool> filter) 
        => _filter = filter ?? throw new ArgumentNullException(nameof(filter));

    public IText Source
    {
        get => _source;
        set
        {
            _baseVersion += 1 + (_source?.Version ?? 0);
            _source = value;
        }
    }

    public int Version => _baseVersion + (Source?.Version ?? 0);
    public IEnumerable<TextBlock> GetBlocks()
    {
        if (Source == null)
            yield break;

        foreach (var block in Source.GetBlocks())
            if (_filter(block))
                yield return block;
    }
}