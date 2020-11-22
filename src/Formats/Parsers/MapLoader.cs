using System;
using SerdesNet;
using UAlbion.Config;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Assets.Maps;

namespace UAlbion.Formats.Parsers
{
    [AssetLoader(FileFormat.MapData)]
    public class MapLoader : IAssetLoader<IMapData>
    {
        public IMapData Serdes(IMapData existing, AssetInfo config, AssetMapping mapping, ISerializer s)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));
            return BaseMapData.Serdes(config, existing, s);
        }

        public object Serdes(object existing, AssetInfo config, AssetMapping mapping, ISerializer s)
            => Serdes(existing as IMapData, config, mapping, s);
    }
}
