using System;
using UAlbion.Api.Visual;
using UAlbion.Config;

namespace UAlbion.Formats.Parsers;

/// <summary>
/// For things like fonts etc that we repack into a texture atlas with buffer pixels.
/// </summary>
public class AtlasPostProcessor : IAssetPostProcessor
{
    const int MarginPixels = 0;

    public object Process(object asset, AssetLoadContext context) => Process((IReadOnlyTexture<byte>)asset, context);
    public static SimpleTexture<byte> Process(IReadOnlyTexture<byte> sprite, AssetLoadContext context)
    {
        if (sprite == null) throw new ArgumentNullException(nameof(sprite));
        if (context == null) throw new ArgumentNullException(nameof(context));

        var layout = SpriteSheetUtil.ArrangeSpriteSheet(sprite.Regions.Count, 1, sprite.GetRegionBuffer);
        if (layout.Layers > 1)
            throw new InvalidOperationException("Could not layout atlas onto one layer");

        var texture = new SimpleTexture<byte>(
            context.AssetId, sprite.Id.ToString(),
            layout.Width, layout.Height);

        var pixelData = texture.GetMutableLayerBuffer(0).Buffer;
        for (int n = 0; n < sprite.Regions.Count; n++)
        {
            var frame = sprite.GetRegionBuffer(n);
            var destWidth = frame.Width + 2 * MarginPixels;
            var destHeight = frame.Height + 2 * MarginPixels;
            var (x, y, _) = layout.Positions[n];
            for (int j = 0; j < destHeight; j++)
            {
                for (int i = 0; i < destWidth; i++)
                {
                    var sourceX = Math.Clamp(i - MarginPixels, 0, frame.Width - 1);
                    var sourceY = Math.Clamp(j - MarginPixels, 0, frame.Height - 1);
                    int src = sourceX + sourceY * sprite.Width;
                    int dest = x + i + (y + j) * layout.Width;
                    byte pixel = frame.Buffer[src];
                    pixelData[dest] = pixel;
                }
            }

            texture.AddRegion(x + MarginPixels, y + MarginPixels, frame.Width, frame.Height);
        }

        return texture;
    }
}