using System;
using System.Collections.Generic;
using SerdesNet;
using UAlbion.Api;
using UAlbion.Config;

namespace UAlbion.Formats.Parsers
{
    [AssetLoader(FileFormat.ItemNames)]
    public class ItemNameLoader : IAssetLoader
    {
        const int StringSize = 20;
        public object Serdes(object existing, AssetInfo config, AssetMapping mapping, ISerializer s)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));
            var streamLength = s.BytesRemaining;
            ApiUtil.Assert(streamLength % StringSize == 0);
            var results = new Dictionary<(int, GameLanguage), string>();
            long end = s.Offset + streamLength;

            int i = 0;
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
    }
}
