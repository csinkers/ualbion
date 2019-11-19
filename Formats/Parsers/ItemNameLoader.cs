using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UAlbion.Formats.Config;

namespace UAlbion.Formats.Parsers
{
    [AssetLoader(FileFormat.ItemNames)]
    public class ItemNameLoader : IAssetLoader
    {
        const int stringSize = 20;
        public object Load(BinaryReader br, long streamLength, string name, AssetConfig.Asset config)
        {
            Debug.Assert(streamLength % stringSize == 0);
            var results = new List<string>(); // Returns item names in the following order: German, English, French.
            long end = br.BaseStream.Position + streamLength;
            while (br.BaseStream.Position < end)
            {
                var bytes = br.ReadBytes(stringSize);
                results.Add(StringUtils.BytesTo850String(bytes));
            }
            return results;
        }
    }
}