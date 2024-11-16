using System;
using System.Collections.Generic;
using System.Linq;
using SerdesNet;
using UAlbion.Api;
using UAlbion.Config;
using UAlbion.Formats.Assets;

namespace UAlbion.Formats.Parsers;

public class ItemNameLoader : IAssetLoader<Dictionary<string, ListStringSet>>
{
    const int StringSize = 20;
    public object Serdes(object existing, ISerdes s, AssetLoadContext context)
        => Serdes((Dictionary<string, ListStringSet>)existing, s, context);

    public Dictionary<string, ListStringSet> Serdes(
        Dictionary<string, ListStringSet> existing,
        ISerdes s,
        AssetLoadContext context)
    {
        ArgumentNullException.ThrowIfNull(s);
        ArgumentNullException.ThrowIfNull(context);

        if (s.IsReading())
            return Read(s);

        ArgumentNullException.ThrowIfNull(existing);

        WriteAll(existing, s);
        return existing;
    }

    static Dictionary<string, ListStringSet> Read(ISerdes s)
    {
        var german = new ListStringSet();
        var english = new ListStringSet();
        var french = new ListStringSet();

        var results = new Dictionary<string, ListStringSet>
            {
                { Base.Language.German, german },
                { Base.Language.English, english },
                { Base.Language.French, french }
            };

        var streamLength = s.BytesRemaining;
        ApiUtil.Assert(streamLength % StringSize == 0, "Expected item name file length to be a whole multiple of the string size");

        long end = s.Offset + streamLength;
        while (s.Offset < end)
        {
            german.Add(s.FixedLengthString(null, null, StringSize));
            english.Add(s.FixedLengthString(null, null, StringSize));
            french.Add(s.FixedLengthString(null, null, StringSize));
        }

        return results;
    }

    static void WriteAll(Dictionary<string, ListStringSet> existing, ISerdes s)
    {
        existing.TryGetValue(Base.Language.German, out var german);
        existing.TryGetValue(Base.Language.English, out var english);
        existing.TryGetValue(Base.Language.French, out var french);

        var maxCount = new[]
        {
            german?.Count,
            english?.Count,
            french?.Count,
        }.Max();

        if (maxCount == null)
            throw new InvalidOperationException($"No strings for base languages in dictionary given to {nameof(ItemNameLoader)}");

        for (int i = 0; i < maxCount.Value; i++)
        {
            Write(german, s, i);
            Write(english, s, i);
            Write(french, s, i);
        }
    }

    static void Write(ListStringSet set, ISerdes s, int i)
    {
        var text = set.Count <= i ? "" : set[i];
        s.FixedLengthString(null, text, StringSize);
    }
}
