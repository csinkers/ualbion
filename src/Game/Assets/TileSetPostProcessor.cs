using UAlbion.Api.Visual;
using UAlbion.Config;
using UAlbion.Formats.Assets.Maps;

namespace UAlbion.Game.Assets;

public class TileSetPostProcessor : IAssetPostProcessor
{
    readonly TilePostProcessor _innerProcessor = new();
    public object Process(object asset, AssetInfo info)
    {
        var texture = (SimpleTexture<byte>)_innerProcessor.Process(asset, info);
        return new SimpleTileGraphics(texture);
    }
}