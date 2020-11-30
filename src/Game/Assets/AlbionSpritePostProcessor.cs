using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using UAlbion.Config;
using UAlbion.Core;
using UAlbion.Core.Textures;
using UAlbion.Formats.Assets;

namespace UAlbion.Game.Assets
{
    public class AlbionSpritePostProcessor : IAssetPostProcessor
    {
        public IEnumerable<Type> SupportedTypes => new[] { typeof(AlbionSprite) };
        public object Process(ICoreFactory factory, AssetId key, object asset)
        {
            if (factory == null) throw new ArgumentNullException(nameof(factory));
            if (asset == null) throw new ArgumentNullException(nameof(asset));
            var sprite = (AlbionSprite)asset;
            SubImage[] subImages;

            // TODO: Put exemptions into assets.json
            if (key.Type == AssetType.Font || key.Type == AssetType.AutomapGraphics || sprite.UniformFrames && sprite.Frames.Count >= 256)
            {
                const int buffer = 1;
                // For things like tilemaps etc we repack into a texture atlas with buffer pixels.
                int srcTileWidth = sprite.Width;
                int srcTileHeight = sprite.Height / sprite.Frames.Count;
                int destTileWidth = srcTileWidth + buffer * 2;
                int destTileHeight = srcTileHeight + buffer * 2;
                var (width, height) = GetAtlasSize(destTileWidth, destTileHeight, sprite.Frames.Count);
                byte[] pixelData = new byte[width * height];
                subImages = new SubImage[sprite.Frames.Count];

                int curX = 0;
                int curY = 0;
                for (int n = 0; n < sprite.Frames.Count; n++)
                {
                    for (int j = 0; j < destTileHeight; j++)
                    {
                        for (int i = 0; i < destTileWidth; i++)
                        {
                            var sourceX = Math.Clamp(i - buffer, 0, srcTileWidth - buffer);
                            var sourceY = Math.Clamp(j - buffer, 0, srcTileHeight - buffer) + n * srcTileHeight;
                            var destX = curX + i;
                            var destY = curY + j;
                            pixelData[destY * width + destX] = sprite.PixelData[sourceX + sourceY * srcTileWidth];
                        }
                    }

                    subImages[n] = new SubImage(
                        new Vector2(curX + buffer, curY + buffer),
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
                    sprite.Name,
                    (uint)width,
                    (uint)height,
                    1,
                    1,
                    pixelData,
                    subImages);
            }
            /*
            else if (sprite.UniformFrames) // For reasonably sized uniform sprites use layers to simplify mip mapping / tiling etc
            {
                int tileWidth = sprite.Width;
                int tileHeight = sprite.Height / sprite.Frames.Count;
                subImages = sprite.Frames
                    .Select((x, i) => new EightBitTexture.SubImage(0, 0, x.Width, x.Height, i))
                    .ToArray();

                return new EightBitTexture(
                    sprite.Name,
                    (uint)tileWidth,
                    (uint)tileHeight,
                    1,
                    (uint)subImages.Length,
                     sprite.PixelData, subImages);
            }*/
            else // For non-uniforms just use the on-disk packing
            {
                subImages = sprite.Frames
                    .Select(x => new SubImage(
                        new Vector2(x.X, x.Y),
                        new Vector2(x.Width, x.Height),
                        new Vector2(sprite.Width, sprite.Height),
                        0))
                    .ToArray();

                return factory.CreateEightBitTexture(
                    sprite.Name,
                    (uint)sprite.Width,
                    (uint)sprite.Height,
                    1,
                    1,
                    sprite.PixelData,
                    subImages);
            }
        }

        static (int, int) GetAtlasSize(int tileWidth, int tileHeight, int count)
        {
            int NextPowerOfTwo(int x) => (int)Math.Pow(2.0, Math.Ceiling(Math.Log(x, 2.0)));

            int tilesPerRow = (int)Math.Ceiling(Math.Sqrt(count));
            int width = NextPowerOfTwo(tileWidth * tilesPerRow);
            int requiredHeight = tileHeight * ((count + tilesPerRow - 1) / tilesPerRow);
            int height = NextPowerOfTwo(requiredHeight);
            return (width, height);
        }
    }
}
