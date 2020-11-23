using SerdesNet;
using UAlbion.Config;
using UAlbion.Formats.Assets.Maps;

namespace UAlbion.Formats.Parsers
{
    public class TilesetLoader : IAssetLoader<TilesetData>
    {
        public TilesetData Serdes(TilesetData existing, AssetInfo config, AssetMapping mapping, ISerializer s)
            => TilesetData.Serdes(null, s, config);

        public object Serdes(object existing, AssetInfo config, AssetMapping mapping, ISerializer s)
            => Serdes(existing as TilesetData, config, mapping, s);
    }
}
