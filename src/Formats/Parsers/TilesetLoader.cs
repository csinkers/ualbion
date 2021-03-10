using SerdesNet;
using UAlbion.Config;
using UAlbion.Formats.Assets.Maps;

namespace UAlbion.Formats.Parsers
{
    public class TilesetLoader : IAssetLoader<TilesetData>
    {
        public TilesetData Serdes(TilesetData existing, AssetInfo info, AssetMapping mapping, ISerializer s)
            => TilesetData.Serdes(existing, s, info);

        public object Serdes(object existing, AssetInfo info, AssetMapping mapping, ISerializer s)
            => Serdes(existing as TilesetData, info, mapping, s);
    }
}
