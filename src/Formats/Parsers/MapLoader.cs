using System;
using SerdesNet;
using UAlbion.Api;
using UAlbion.Config;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Assets.Maps;

namespace UAlbion.Formats.Parsers
{
    public class MapLoader : IAssetLoader<IMapData>
    {
        public IMapData Serdes(IMapData existing, AssetInfo info, AssetMapping mapping, ISerializer s, IJsonUtil jsonUtil)
        {
            if (info == null) throw new ArgumentNullException(nameof(info));
            return BaseMapData.Serdes(info, existing, mapping, s);
        }

        public object Serdes(object existing, AssetInfo info, AssetMapping mapping, ISerializer s, IJsonUtil jsonUtil)
            => Serdes(existing as IMapData, info, mapping, s, jsonUtil);
    }
}
