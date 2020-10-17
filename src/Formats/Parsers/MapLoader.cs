using System;
using System.IO;
using SerdesNet;
using UAlbion.Config;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Assets.Maps;

namespace UAlbion.Formats.Parsers
{
    [AssetLoader(FileFormat.MapData)]
    public class MapLoader : IAssetLoader<IMapData>
    {
        public IMapData Serdes(IMapData existing, AssetMapping mapping, ISerializer s, AssetId id, AssetInfo config)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));
            return BaseMapData.Serdes(config.Id, existing, s);
        }

        public object Load(BinaryReader br, long streamLength, AssetMapping mapping, AssetId id, AssetInfo config)
            => Serdes(null, mapping, new AlbionReader(br, streamLength), id, config);
    }
}
