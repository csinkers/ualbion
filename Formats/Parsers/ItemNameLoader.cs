using System.Collections.Generic;
using System.IO;
using UAlbion.Formats.Config;

namespace UAlbion.Formats.Parsers
{
    [AssetLoader(XldObjectType.ItemNames)]
    public class ItemNameLoader : IAssetLoader
    {
        public object Load(BinaryReader br, long streamLength, string name, AssetConfig.Asset config)
        {
            var results = new List<string>(); // Returns item names in the following order: German, English, French.
            long end = br.BaseStream.Position + streamLength;
            while (br.BaseStream.Position < end)
            {
                var bytes = br.ReadBytes(20);
                results.Add(StringUtils.BytesTo850String(bytes));
            }
            return results;
        }
    }
}