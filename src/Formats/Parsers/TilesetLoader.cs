using SerdesNet;
using UAlbion.Config;
using UAlbion.Config.Properties;
using UAlbion.Formats.Assets.Maps;

namespace UAlbion.Formats.Parsers;

public class TilesetLoader : IAssetLoader<TilesetData>
{
    public static readonly BoolAssetProperty UseSmallGraphicsProperty = new("UseSmallGraphics"); 
    public TilesetData Serdes(TilesetData existing, ISerializer s, AssetLoadContext context)
        => TilesetData.Serdes(existing, s, context);

    public object Serdes(object existing, ISerializer s, AssetLoadContext context)
        => Serdes(existing as TilesetData, s, context);
}