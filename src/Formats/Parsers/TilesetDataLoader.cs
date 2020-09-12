using System;
using System.IO;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Assets.Maps;
using UAlbion.Formats.Config;

namespace UAlbion.Formats.Parsers
{
    [AssetLoader(FileFormat.Tileset)]
    public class TilesetDataLoader : IAssetLoader
    {
        public object Load(BinaryReader br, long streamLength, AssetKey key, AssetInfo config)
        {
            if (br == null) throw new ArgumentNullException(nameof(br));
            if (config == null) throw new ArgumentNullException(nameof(config));
            return TilesetData.Serdes(null, new AlbionReader(br, streamLength), config);
        }
    }
}
