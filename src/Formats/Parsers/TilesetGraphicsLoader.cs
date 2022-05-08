using SerdesNet;
using UAlbion.Api.Visual;
using UAlbion.Config;
using UAlbion.Formats.Assets.Maps;

namespace UAlbion.Formats.Parsers;

public class TilesetGraphicsLoader : IAssetLoader<ITileGraphics>
{
    readonly FixedSizeSpriteLoader _spriteLoader = new();

    public ITileGraphics Serdes(ITileGraphics existing, AssetInfo info, ISerializer s, SerdesContext context)
    {
        var asset = _spriteLoader.Serdes((IReadOnlyTexture<byte>)existing?.Texture, info, s, context);

        if (s.IsWriting())
            return existing;

        var texture = AtlasPostProcessor.Process(asset, info);
        return new SimpleTileGraphics(texture);
    }

    public object Serdes(object existing, AssetInfo info, ISerializer s, SerdesContext context)
        => Serdes((ITileGraphics)existing, info, s, context);
}