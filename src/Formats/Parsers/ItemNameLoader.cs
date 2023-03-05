using System;
using SerdesNet;
using UAlbion.Api;
using UAlbion.Config;
using UAlbion.Formats.Assets;

namespace UAlbion.Formats.Parsers;

public class ItemNameLoader : IAssetLoader<MultiLanguageStringDictionary>
{
    const int StringSize = 20;
    public object Serdes(object existing, AssetInfo info, ISerializer s, SerdesContext context)
        => Serdes((MultiLanguageStringDictionary)existing, info, s, context);

    public MultiLanguageStringDictionary Serdes(MultiLanguageStringDictionary existing, AssetInfo info, ISerializer s, SerdesContext context)
    {
        if (s == null) throw new ArgumentNullException(nameof(s));
        if (s.IsWriting() && existing == null) throw new ArgumentNullException(nameof(existing));

        existing ??= new MultiLanguageStringDictionary();
        if (!existing.ContainsKey(Base.Language.German)) existing[Base.Language.German] = new ListStringSet();
        if (!existing.ContainsKey(Base.Language.English)) existing[Base.Language.English] = new ListStringSet();
        if (!existing.ContainsKey(Base.Language.French)) existing[Base.Language.French] = new ListStringSet();

        static void Inner(IStringSet collection, int i, ISerializer s2)
        {
            var concrete = (ListStringSet)collection;
            while (collection.Count <= i)
                concrete.Add(null);
            concrete[i] = s2.FixedLengthString(null, concrete[i], StringSize);
        }

        if (s.IsReading())
        {
            var streamLength = s.BytesRemaining;
            ApiUtil.Assert(streamLength % StringSize == 0, "Expected item name file length to be a whole multiple of the string size");

            int i = 1;
            long end = s.Offset + streamLength;
            while (s.Offset < end)
            {
                Inner(existing[Base.Language.German], i, s);
                Inner(existing[Base.Language.English], i, s);
                Inner(existing[Base.Language.French], i, s);
                i++;
            }
        }
        else
        {
            var stringCount = existing[Base.Language.German].Count;
            for (int i = 1; i < stringCount; i++)
            {
                Inner(existing[Base.Language.German], i, s);
                Inner(existing[Base.Language.English], i, s);
                Inner(existing[Base.Language.French], i, s);
            }
        }

        return existing;
    }
}
