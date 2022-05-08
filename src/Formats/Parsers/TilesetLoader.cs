using SerdesNet;
using UAlbion.Config;
using UAlbion.Formats.Assets.Maps;

namespace UAlbion.Formats.Parsers;

public class TilesetLoader : IAssetLoader<TilesetData>
{
    public TilesetData Serdes(TilesetData existing, AssetInfo info, ISerializer s, SerdesContext context)
        => TilesetData.Serdes(existing, s, info);

    public object Serdes(object existing, AssetInfo info, ISerializer s, SerdesContext context)
        => Serdes(existing as TilesetData, info, s, context);
}