using System;
using System.Collections.Generic;
using System.Linq;
using SerdesNet;
using UAlbion.Api;
using UAlbion.Config;

namespace UAlbion.Formats.Parsers
{
    public class ItemNameLoader : IAssetLoader<IDictionary<(int, GameLanguage), string>>
    {
        const int StringSize = 20;
        public object Serdes(object existing, AssetInfo config, AssetMapping mapping, ISerializer s) 
            => Serdes((IDictionary<(int, GameLanguage), string>) existing, config, mapping, s);

        public IDictionary<(int, GameLanguage), string> Serdes(IDictionary<(int, GameLanguage), string> existing, AssetInfo config, AssetMapping mapping, ISerializer s)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));
            if (s.IsReading())
            {
                var streamLength = s.BytesRemaining;
                ApiUtil.Assert(streamLength % StringSize == 0);
                var results = new Dictionary<(int, GameLanguage), string>();
                long end = s.Offset + streamLength;

                int i = 3;
                while (s.Offset < end)
                {
                    var bytes = s.ByteArray(null, null, StringSize);
                    var language = (i % 3) switch
                    {
                        0 => GameLanguage.German,
                        1 => GameLanguage.English,
                        _ => GameLanguage.French,
                    };

                    results[(i / 3, language)] = FormatUtil.BytesTo850String(bytes);
                    i++;
                }
                return results;
            }

            if (existing == null) throw new ArgumentNullException(nameof(existing));
            int maxId = existing.Keys.Select(x => x.Item1).Max();
            for (int i = 1; i <= maxId; i++)
            {
                if (!existing.TryGetValue((i, GameLanguage.German), out var value)) { } 
                s.FixedLengthString(null, value, StringSize);
                if (!existing.TryGetValue((i, GameLanguage.English), out value)) { } 
                s.FixedLengthString(null, value, StringSize);
                if (!existing.TryGetValue((i, GameLanguage.French), out value)) { } 
                s.FixedLengthString(null, value, StringSize);
            }

            return existing;
        }
    }
}
