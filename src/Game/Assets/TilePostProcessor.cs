using System;
using System.Numerics;
using UAlbion.Api;
using UAlbion.Config;
using UAlbion.Core;
using UAlbion.Core.Textures;
using UAlbion.Formats.Assets;

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

            var sprite = (AlbionSprite2)asset;

            int srcTileWidth = sprite.Width;
            int srcTileHeight = sprite.Height / sprite.Frames.Count;
            int destTileWidth = srcTileWidth + MarginPixels * 2;
            int destTileHeight = srcTileHeight + MarginPixels * 2;
            var (width, height) = GetAtlasSize(destTileWidth, destTileHeight, sprite.Frames.Count);
            byte[] pixelData = new byte[width * height];
            var subImages = new SubImage[sprite.Frames.Count];

            int curX = 0;
            int curY = 0;
            for (int n = 0; n < sprite.Frames.Count; n++)
            {
                for (int j = 0; j < destTileHeight; j++)
                {
                    for (int i = 0; i < destTileWidth; i++)
                    {
                        var sourceX = Math.Clamp(i - MarginPixels, 0, srcTileWidth - MarginPixels);
                        var sourceY = Math.Clamp(j - MarginPixels, 0, srcTileHeight - MarginPixels) + n * srcTileHeight;
                        var destX = curX + i;
                        var destY = curY + j;
                        pixelData[destY * width + destX] = sprite.PixelData[sourceX + sourceY * srcTileWidth];
                    }
                }

                subImages[n] = new SubImage(
                    new Vector2(curX + MarginPixels, curY + MarginPixels),
                    new Vector2(sprite.Frames[n].Width, sprite.Frames[n].Height),
                    new Vector2(width, height),
                    0);

                curX += destTileWidth;
                if (curX + destTileWidth > width)
                {
                    curX = 0;
                    curY += destTileHeight;
                }
            }

            return factory.CreateEightBitTexture(
                info.AssetId,
                sprite.Id.ToString(),
                width,
                height,
                1,
                1,
                pixelData,
                subImages);
        }

        static (int, int) GetAtlasSize(int tileWidth, int tileHeight, int count)
        {
            int tilesPerRow = (int)Math.Ceiling(Math.Sqrt(count));
            int width = ApiUtil.NextPowerOfTwo(tileWidth * tilesPerRow);
            int requiredHeight = tileHeight * ((count + tilesPerRow - 1) / tilesPerRow);
            int height = ApiUtil.NextPowerOfTwo(requiredHeight);
            return (width, height);
        }
    }
}