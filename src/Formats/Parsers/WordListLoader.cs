using System;
using System.Collections.Generic;
using SerdesNet;
using UAlbion.Api;
using UAlbion.Config;
using UAlbion.Formats.Assets;

namespace UAlbion.Formats.Parsers;

public class WordListLoader : IAssetLoader<ListStringCollection>
{
    const int WordLength = 21;

    public object Serdes(object existing, AssetInfo info, ISerializer s, LoaderContext context)
        => Serdes((ListStringCollection)existing, info, s, context);

    public ListStringCollection Serdes(ListStringCollection existing, AssetInfo info, ISerializer s, LoaderContext context)
    {
        if (s == null) throw new ArgumentNullException(nameof(s));
        if (info == null) throw new ArgumentNullException(nameof(info));

        if (s.IsReading())
        {
            ApiUtil.Assert(s.BytesRemaining % WordLength == 0, "Expected word list file length to be a whole multiple of the string size");
            var strings = new List<string>();
            while (!s.IsComplete())
                strings.Add(s.FixedLengthString(null, null, WordLength));
            return new ListStringCollection(strings);
        }
        else
        {
            if (existing == null)
                throw new ArgumentNullException(nameof(existing));

            foreach (var x in existing)
            {
                if (x is { Length: > WordLength })
                    throw new ArgumentOutOfRangeException(nameof(existing), $"Tried to write a word ({x}) of length {x.Length} to a word list, but the maximum length is {WordLength}");

                s.FixedLengthString(null, x, WordLength);
            }

            return existing;
        }
    }
}
