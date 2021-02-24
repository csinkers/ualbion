using System;
using System.Collections.Generic;
using SerdesNet;
using UAlbion.Api;
using UAlbion.Config;
using UAlbion.Formats.Assets;

namespace UAlbion.Formats.Parsers
{
    public class ItemNameLoader : IAssetLoader<IDictionary<GameLanguage, StringCollection>>
    {
        const int StringSize = 20;
        public object Serdes(object existing, AssetInfo config, AssetMapping mapping, ISerializer s)
            => Serdes((IDictionary<GameLanguage, StringCollection>)existing, config, mapping, s);

        public IDictionary<GameLanguage, StringCollection> Serdes(IDictionary<GameLanguage, StringCollection> names, AssetInfo config, AssetMapping mapping, ISerializer s)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));
            var streamLength = s.BytesRemaining;
            ApiUtil.Assert(streamLength % StringSize == 0);
            names ??= new Dictionary<GameLanguage, StringCollection>();
            if (!names.ContainsKey(GameLanguage.German)) names[GameLanguage.German] = new StringCollection();
            if (!names.ContainsKey(GameLanguage.English)) names[GameLanguage.English] = new StringCollection();
            if (!names.ContainsKey(GameLanguage.French)) names[GameLanguage.French] = new StringCollection();

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
                    Inner(names[GameLanguage.German], i, s);
                    Inner(names[GameLanguage.English], i, s);
                    Inner(names[GameLanguage.French], i, s);
                    i++;
                }
            }
            else
            {
                var stringCount = names[GameLanguage.German].Count;
                for (int i = 1; i < stringCount; i++)
                {
                    Inner(names[GameLanguage.German], i, s);
                    Inner(names[GameLanguage.English], i, s);
                    Inner(names[GameLanguage.French], i, s);
                }
            }

            return names;
        }
    }
}
