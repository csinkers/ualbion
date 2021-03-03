using System;
using System.Collections.Generic;
using SerdesNet;
using UAlbion.Api;
using UAlbion.Config;
using UAlbion.Formats.Assets;

namespace UAlbion.Formats.Parsers
{
    public class ItemNameLoader : IAssetLoader<IDictionary<string, StringCollection>>
    {
        const int StringSize = 20;
        public object Serdes(object existing, AssetInfo config, AssetMapping mapping, ISerializer s)
            => Serdes((IDictionary<string, StringCollection>)existing, config, mapping, s);

        public IDictionary<string, StringCollection> Serdes(IDictionary<string, StringCollection> names, AssetInfo config, AssetMapping mapping, ISerializer s)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));
            var streamLength = s.BytesRemaining;
            ApiUtil.Assert(streamLength % StringSize == 0);
            names ??= new Dictionary<string, StringCollection>();
            if (!names.ContainsKey(Base.Language.German)) names[Base.Language.German] = new StringCollection();
            if (!names.ContainsKey(Base.Language.English)) names[Base.Language.English] = new StringCollection();
            if (!names.ContainsKey(Base.Language.French)) names[Base.Language.French] = new StringCollection();

            static void Inner(StringCollection collection, int i, ISerializer s2)
            {
                while (collection.Count <= i)
                    collection.Add(null);
                collection[i] = s2.FixedLengthString(null, collection[i], StringSize);
            }

            if (s.IsReading())
            {
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
