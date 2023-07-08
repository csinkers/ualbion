using System;
using SerdesNet;
using UAlbion.Api;
using UAlbion.Config;
using UAlbion.Formats.Assets;

namespace UAlbion.Formats.Parsers;

public class ItemNameLoader : IAssetLoader<ListStringSet>
{
    const int StringSize = 20;
    public object Serdes(object existing, ISerializer s, AssetLoadContext context)
        => Serdes((ListStringSet)existing, s, context);

    public ListStringSet Serdes(ListStringSet existing, ISerializer s, AssetLoadContext context)
    {
        if (s == null) throw new ArgumentNullException(nameof(s));
        if (context == null) throw new ArgumentNullException(nameof(context));
        if (s.IsWriting() && existing == null) throw new ArgumentNullException(nameof(existing));

        int? langIndex = context.Language switch
        {
            Base.Language.German  => 0,
            Base.Language.English => 1,
            Base.Language.French  => 2,
            _ => null
        };

        if (langIndex == null)
            return existing;

        existing ??= new ListStringSet();

        string[] strings = new string[3];
        if (s.IsReading())
        {
            var streamLength = s.BytesRemaining;
            ApiUtil.Assert(streamLength % StringSize == 0, "Expected item name file length to be a whole multiple of the string size");

            long end = s.Offset + streamLength;
            while (s.Offset < end)
            {
                strings[0] = s.FixedLengthString(null, null, StringSize);
                strings[1] = s.FixedLengthString(null, null, StringSize);
                strings[2] = s.FixedLengthString(null, null, StringSize);
                existing.Add(strings[langIndex.Value]);
            }
        }
        else
        {
            for (int i = 0; i < existing.Count; i++)
            {
                WriteOrSkip(existing[i], s, langIndex == 0);
                WriteOrSkip(existing[i], s, langIndex == 1);
                WriteOrSkip(existing[i], s, langIndex == 2);
            }
        }

        return existing;
    }

    static void WriteOrSkip(string existing, ISerializer s2, bool write)
    {
        if (write)
        {
            s2.FixedLengthString(null, existing, StringSize);
        }
        else
        {
            if (s2.BytesRemaining == 0)
                s2.FixedLengthString(null, "", StringSize);
            else
                s2.Seek(s2.Offset + StringSize);
        }
    }
}
