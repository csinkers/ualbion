using System;
using SerdesNet;
using UAlbion.Api;
using UAlbion.Config;
using UAlbion.Formats.Assets;

namespace UAlbion.Formats.Parsers
{
    public class ItemNameLoader : IAssetLoader<MultiLanguageStringDictionary>
    {
        const int StringSize = 20;
        public object Serdes(object existing, AssetInfo info, AssetMapping mapping, ISerializer s)
            => Serdes((MultiLanguageStringDictionary)existing, info, mapping, s);

        public MultiLanguageStringDictionary Serdes(MultiLanguageStringDictionary names, AssetInfo info, AssetMapping mapping, ISerializer s)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));
            if (s.IsWriting() && names == null) throw new ArgumentNullException(nameof(names));

            names ??= new MultiLanguageStringDictionary();
            if (!names.ContainsKey(Base.Language.German)) names[Base.Language.German] = new ListStringCollection();
            if (!names.ContainsKey(Base.Language.English)) names[Base.Language.English] = new ListStringCollection();
            if (!names.ContainsKey(Base.Language.French)) names[Base.Language.French] = new ListStringCollection();

            static void Inner(IStringCollection collection, int i, ISerializer s2)
            {
                var concrete = (ListStringCollection)collection;
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
                    Inner(names[Base.Language.German], i, s);
                    Inner(names[Base.Language.English], i, s);
                    Inner(names[Base.Language.French], i, s);
                    i++;
                }
            }
            else
            {
                var stringCount = names[Base.Language.German].Count;
                for (int i = 1; i < stringCount; i++)
                {
                    Inner(names[Base.Language.German], i, s);
                    Inner(names[Base.Language.English], i, s);
                    Inner(names[Base.Language.French], i, s);
                }
            }

            return names;
        }
    }
}
