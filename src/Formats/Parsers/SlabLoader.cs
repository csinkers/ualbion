using System;
using SerdesNet;
using UAlbion.Api;
using UAlbion.Api.Visual;
using UAlbion.Config;

namespace UAlbion.Formats.Parsers;

public class SlabLoader : IAssetLoader<IReadOnlyTexture<byte>>
{
    const int StatusBarHeight = 48;
    public object Serdes(object existing, AssetInfo info, AssetMapping mapping, ISerializer s, IJsonUtil jsonUtil)
        => Serdes((IReadOnlyTexture<byte>)existing, info, mapping, s, jsonUtil);

    public IReadOnlyTexture<byte> Serdes(IReadOnlyTexture<byte> existing, AssetInfo info, AssetMapping mapping, ISerializer s, IJsonUtil jsonUtil)
    {
        IReadOnlyTexture<byte> singleFrame = null;
        if (s.IsWriting())
        {
            if (existing == null) throw new ArgumentNullException(nameof(existing));
            singleFrame =
                new SimpleTexture<byte>(
                        existing.Id,
                        existing.Name,
                        existing.Width,
                        existing.Height,
                        existing.PixelData.ToArray())
                    .AddRegion(existing.Regions[0].X, existing.Regions[0].Y, existing.Regions[0].Width, existing.Regions[0].Height);
        }

        var sprite = new FixedSizeSpriteLoader().Serdes(singleFrame, info, mapping, s, jsonUtil);
        if (sprite == null)
            return null;

        return new SimpleTexture<byte>(
                sprite.Id,
                sprite.Name,
                sprite.Width,
                sprite.Height,
                sprite.PixelData.ToArray())
            .AddRegion(0, 0, sprite.Width, sprite.Height)
            .AddRegion(0, sprite.Height - StatusBarHeight, sprite.Width, StatusBarHeight);
    }
}