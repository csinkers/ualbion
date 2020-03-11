using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using UAlbion.Core;
using UAlbion.Core.Textures;
using UAlbion.Formats.Assets;

namespace UAlbion.Game.Assets
{
    public class AlbionSpritePostProcessor : IAssetPostProcessor
    {
        public IEnumerable<Type> SupportedTypes => new[] { typeof(AlbionSprite) };
        public object Process(ICoreFactory factory, string name, object asset)
        {
            var sprite = (AlbionSprite)asset;
            SubImage[] subImages;
            byte[] pixelData;

            if (sprite.UniformFrames && sprite.Frames.Count >= 256)
            {
                // For things like tilemaps etc we repack into a power of 2-aligned texture atlas.
                int tileWidth = sprite.Width;
                int tileHeight = sprite.Height / sprite.Frames.Count;
                var (width, height) = GetAtlasSize(tileWidth, tileHeight, sprite.Frames.Count);
                pixelData = new byte[width * height];
                subImages = new SubImage[sprite.Frames.Count];

                int curX = 0;
                int curY = 0;
                for (int n = 0; n < sprite.Frames.Count; n++)
                {
                    for (int j = 0; j < tileHeight; j++)
                    {
                        for (int i = 0; i < tileWidth; i++)
                        {
                            var sourceX = i;
                            var sourceY = j + n * tileHeight;
                            var destX = curX + i;
                            var destY = curY + j;
                            pixelData[destY * width + destX] = sprite.PixelData[sourceX + sourceY * tileWidth];
                        }
                    }

                    subImages[n] = new SubImage(
                        new Vector2(curX, curY),
                        new Vector2(tileWidth, tileHeight),
                        new Vector2(width, height),
                        0);

                    curX += tileWidth;
                    if (curX + tileWidth > width)
                    {
                        curX = 0;
                        curY += tileHeight;
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
                pixelData = sprite.PixelData;
                subImages = sprite.Frames
                    .Select((x, i) => new EightBitTexture.SubImage(0, 0, x.Width, x.Height, i))
                    .ToArray();

                return new EightBitTexture(
                    sprite.Name,
                    (uint)tileWidth,
                    (uint)tileHeight,
                    1,
                    (uint)subImages.Length,
                    pixelData, subImages);
            }*/
            else // For non-uniforms just use the on-disk packing 
            {
                pixelData = sprite.PixelData;
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
                    pixelData,
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