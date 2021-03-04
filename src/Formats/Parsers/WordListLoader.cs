using System;
using System.Collections.Generic;
using SerdesNet;
using UAlbion.Api;
using UAlbion.Config;
using UAlbion.Formats.Assets;

namespace UAlbion.Formats.Parsers
{
    public class WordListLoader : IAssetLoader<ListStringCollection>
    {
        const int WordLength = 21;

        public object Serdes(object existing, AssetInfo config, AssetMapping mapping, ISerializer s)
            => Serdes((ListStringCollection)existing, config, mapping, s);

        public ListStringCollection Serdes(ListStringCollection existing, AssetInfo config, AssetMapping mapping, ISerializer s)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));
            if (config == null) throw new ArgumentNullException(nameof(config));

            ApiUtil.Assert(s.BytesRemaining % WordLength == 0);

            if (s.IsReading())
            {
                var strings = new List<string>();
                while (!s.IsComplete())
                    strings.Add(s.FixedLengthString(null, null, WordLength));
                return new ListStringCollection(strings);
            }
            else
            {
                if(existing == null)
                    throw new ArgumentNullException(nameof(existing));

                foreach (var x in existing)
                    s.FixedLengthString(null, x, WordLength);

                return existing;
            }
        }
    }
}
