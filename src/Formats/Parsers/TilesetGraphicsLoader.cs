using SerdesNet;
using UAlbion.Api.Visual;
using UAlbion.Config;
using UAlbion.Formats.Assets.Maps;

namespace UAlbion.Formats.Parsers;

public class TilesetGraphicsLoader : IAssetLoader<ITileGraphics>
{
    readonly FixedSizeSpriteLoader _spriteLoader = new();

    public ITileGraphics Serdes(ITileGraphics existing, ISerializer s, AssetLoadContext context)
    {
        var asset = _spriteLoader.Serdes((IReadOnlyTexture<byte>)existing?.Texture, s, context);

        if (s.IsWriting())
            return existing;

        var texture = AtlasPostProcessor.Process(asset, context);
        return new SimpleTileGraphics(texture);
    }

    public object Serdes(object existing, ISerializer s, AssetLoadContext context)
        => Serdes((ITileGraphics)existing, s, context);
}