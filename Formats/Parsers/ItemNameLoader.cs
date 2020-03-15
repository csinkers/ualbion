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
            var results = new List<string>(); // Returns item names in the following order: German, English, French.
            long end = br.BaseStream.Position + streamLength;
            while (br.BaseStream.Position < end)
            {
                var bytes = br.ReadBytes(StringSize);
                results.Add(StringUtils.BytesTo850String(bytes));
            }
            return results;
        }
    }
}
