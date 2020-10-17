using System;
using System.IO;
using UAlbion.Config;
using UAlbion.Formats.Assets.Maps;

namespace UAlbion.Formats.Parsers
{
    [AssetLoader(FileFormat.Tileset)]
    public class TilesetDataLoader : IAssetLoader
    {
        public object Load(BinaryReader br, long streamLength, AssetMapping mapping, AssetId id, AssetInfo config)
        {
            if (br == null) throw new ArgumentNullException(nameof(br));
            if (config == null) throw new ArgumentNullException(nameof(config));
            return TilesetData.Serdes(null, new AlbionReader(br, streamLength), config);
        }
    }
}
