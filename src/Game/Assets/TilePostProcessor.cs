﻿using System;
using System.Numerics;
using UAlbion.Api;
using UAlbion.Api.Visual;
using UAlbion.Config;
using UAlbion.Core;
using UAlbion.Core.Textures;

namespace UAlbion.Game.Assets
{
    /// <summary>
    /// For things like tilemaps etc we repack into a texture atlas with buffer pixels.
    /// </summary>
    public class TilePostProcessor : IAssetPostProcessor
    {
        const int MarginPixels = 1;
        public object Process(object asset, AssetInfo info, ICoreFactory factory)
        {
            if (asset == null) throw new ArgumentNullException(nameof(asset));
            if (info == null) throw new ArgumentNullException(nameof(info));
            if (factory == null) throw new ArgumentNullException(nameof(factory));

            var sprite = (IEightBitImage)asset;
            var layout = SpriteSheetUtil.ArrangeSpriteSheet(sprite.SubImageCount, 1, sprite.GetSubImageBuffer);
            var totalHeight = ApiUtil.NextPowerOfTwo(layout.Height);
            byte[] pixelData = new byte[layout.Width * totalHeight];
            var subImages = new SubImage[sprite.SubImageCount];

            for (int n = 0; n < sprite.SubImageCount; n++)
            {
                var frame = sprite.GetSubImageBuffer(n);
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

                subImages[n] = new SubImage(
                    new Vector2(x + MarginPixels, y + MarginPixels),
                    new Vector2(frame.Width, frame.Height),
                    new Vector2(layout.Width, totalHeight),
                    0);
            }

            return factory.CreateEightBitTexture(
                info.AssetId,
                sprite.Id.ToString(),
                layout.Width,
                totalHeight,
                1,
                1,
                pixelData,
                subImages);
        }
    }
}