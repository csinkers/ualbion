using System;
using System.IO;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Config;

namespace UAlbion.Formats.Parsers
{
    [AssetLoader(FileFormat.BlockList)]
    public class BlockListLoader : IAssetLoader
    {
        public object Load(BinaryReader br, long streamLength, AssetKey key, AssetInfo config)
        {
            if (br == null) throw new ArgumentNullException(nameof(br));
            return Block.Serdes(key.Id, null, new AlbionReader(br, streamLength));
        }
    }
}
