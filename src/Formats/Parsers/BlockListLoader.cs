using System;
using System.IO;
using UAlbion.Config;
using UAlbion.Formats.Assets;

namespace UAlbion.Formats.Parsers
{
    [AssetLoader(FileFormat.BlockList)]
    public class BlockListLoader : IAssetLoader
    {
        public object Load(BinaryReader br, long streamLength, AssetMapping mapping, AssetId id, AssetInfo config)
        {
            if (br == null) throw new ArgumentNullException(nameof(br));
            return Block.Serdes(id.Id, null, new AlbionReader(br, streamLength));
        }
    }
}
