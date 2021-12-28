using System;
using System.Numerics;
using UAlbion.Api;
using UAlbion.Api.Visual;
using UAlbion.Config;

namespace UAlbion.Game.Assets;

/// <summary>
/// For things like tilemaps etc we repack into a texture atlas with buffer pixels.
/// </summary>
public class TilePostProcessor : IAssetPostProcessor
{
    const int MarginPixels = 1;
    public object Process(object asset, AssetInfo info)
    {
        if (asset == null) throw new ArgumentNullException(nameof(asset));
        if (info == null) throw new ArgumentNullException(nameof(info));

        var sprite = (IReadOnlyTexture<byte>)asset;
        var layout = SpriteSheetUtil.ArrangeSpriteSheet(sprite.Regions.Count, 1, sprite.GetRegionBuffer);
        var totalHeight = ApiUtil.NextPowerOfTwo(layout.Height);
        byte[] pixelData = new byte[layout.Width * totalHeight];
        var subImages = new Region[sprite.Regions.Count];

        for (int n = 0; n < sprite.Regions.Count; n++)
        {
            var frame = sprite.GetRegionBuffer(n);
            var destWidth = frame.Width + 2 * MarginPixels;
            var destHeight = frame.Height + 2 * MarginPixels;
            var (x, y) = layout.Positions[n];
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

            subImages[n] = new Region(
                new Vector2(x + MarginPixels, y + MarginPixels),
                new Vector2(frame.Width, frame.Height),
                new Vector2(layout.Width, totalHeight),
                0);
        }

        return new SimpleTexture<byte>(
            info.AssetId, sprite.Id.ToString(),
            layout.Width, totalHeight,
            pixelData,
            subImages);
    }
}