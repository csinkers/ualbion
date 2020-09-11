using System;
using System.IO;
using SerdesNet;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Assets.Maps;
using UAlbion.Formats.Config;

namespace UAlbion.Formats.Parsers
{
    [AssetLoader(FileFormat.MapData)]
    public class MapLoader : IAssetLoader<IMapData>
    {
        public IMapData Serdes(IMapData existing, ISerializer s, AssetKey key, AssetInfo config)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));
            return BaseMapData.Serdes(config.Id, existing, s);
        }

        public object Load(BinaryReader br, long streamLength, AssetKey key, AssetInfo config)
            => Serdes(null, new AlbionReader(br, streamLength), key, config);
    }
}
