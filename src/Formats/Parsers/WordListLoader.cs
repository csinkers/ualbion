using System;
using System.Collections.Generic;
using SerdesNet;
using UAlbion.Api;
using UAlbion.Config;
using UAlbion.Formats.Assets;

namespace UAlbion.Formats.Parsers;

public class WordListLoader : IAssetLoader<ListStringSet>
{
    const int WordLength = 21;

    public object Serdes(object existing, ISerializer s, AssetLoadContext context)
        => Serdes((ListStringSet)existing, s, context);

    public ListStringSet Serdes(ListStringSet existing, ISerializer s, AssetLoadContext context)
    {
        if (s == null) throw new ArgumentNullException(nameof(s));
        if (context == null) throw new ArgumentNullException(nameof(context));

        if (s.IsReading())
        {
            ApiUtil.Assert(s.BytesRemaining % WordLength == 0, "Expected word list file length to be a whole multiple of the string size");
            var strings = new List<string>();
            while (s.BytesRemaining > 0)
                strings.Add(s.FixedLengthString(null, null, WordLength));

            return new ListStringSet(strings);
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
