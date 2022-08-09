using System;
using SerdesNet;
using UAlbion.Api.Visual;
using UAlbion.Config;

namespace UAlbion.Formats.Parsers;

public class SlabLoader : IAssetLoader<IReadOnlyTexture<byte>>
{
    const int StatusBarHeight = 48;
    public object Serdes(object existing, AssetInfo info, ISerializer s, SerdesContext context)
        => Serdes((IReadOnlyTexture<byte>)existing, info, s, context);

    public IReadOnlyTexture<byte> Serdes(IReadOnlyTexture<byte> existing, AssetInfo info, ISerializer s, SerdesContext context)
    {
        IReadOnlyTexture<byte> singleFrame = null;
        if (s.IsWriting())
        {
            if (existing == null) throw new ArgumentNullException(nameof(existing));
            var texture = new SimpleTexture<byte>(
                        existing.Id,
                        existing.Name,
                        existing.Width,
                        existing.Height,
                        existing.PixelData.ToArray());

            texture.AddRegion(existing.Regions[0].X, existing.Regions[0].Y, existing.Regions[0].Width, existing.Regions[0].Height);
            singleFrame = texture;
        }

        var sprite = new FixedSizeSpriteLoader().Serdes(singleFrame, info, s, context);
        if (sprite == null)
            return null;

        var result = new SimpleTexture<byte>(sprite.Id, sprite.Name, sprite.Width, sprite.Height, sprite.PixelData.ToArray());
        result.AddRegion(0, 0, sprite.Width, sprite.Height);
        result.AddRegion(0, sprite.Height - StatusBarHeight, sprite.Width, StatusBarHeight);
        return result;
    }
}