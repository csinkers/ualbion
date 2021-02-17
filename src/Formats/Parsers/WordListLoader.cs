using System;
using System.Collections.Generic;
using SerdesNet;
using UAlbion.Api;
using UAlbion.Config;
using UAlbion.Formats.Assets;

namespace UAlbion.Formats.Parsers
{
    public class WordListLoader : IAssetLoader<StringCollection>
    {
        const int WordLength = 21;

        public object Serdes(object existing, AssetInfo config, AssetMapping mapping, ISerializer s)
            => Serdes((StringCollection)existing, config, mapping, s);

        public StringCollection Serdes(StringCollection existing, AssetInfo config, AssetMapping mapping, ISerializer s)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));
            if (config == null) throw new ArgumentNullException(nameof(config));

            ApiUtil.Assert(s.BytesRemaining % WordLength == 0);

            if (s.IsReading())
            {
                var strings = new List<string>();
                while (!s.IsComplete())
                    strings.Add(s.FixedLengthString(null, null, WordLength));
                return new StringCollection(strings.ToArray());
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
