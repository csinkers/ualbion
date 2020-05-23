using System.Collections.Generic;
using System.IO;
using UAlbion.Api;
using UAlbion.Formats.Config;

namespace UAlbion.Formats.Parsers
{
    [AssetLoader(FileFormat.ItemNames)]
    public class ItemNameLoader : IAssetLoader
    {
        const int StringSize = 20;
        public object Load(BinaryReader br, long streamLength, string name, AssetInfo config)
        {
            ApiUtil.Assert(streamLength % StringSize == 0);
            var results = new Dictionary<(int, GameLanguage), string>();
            long end = br.BaseStream.Position + streamLength;

            int i = 0;
            while (br.BaseStream.Position < end)
            {
                var bytes = br.ReadBytes(StringSize);
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
