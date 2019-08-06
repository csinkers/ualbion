using System.Diagnostics;
using System.IO;

namespace UAlbion.Formats.Parsers
{
    [AssetLoader(XldObjectType.FixedSizeSprite)]
    public class FixedSizeSpriteLoader : IAssetLoader
    {
        public object Load(BinaryReader br, long streamLength, string name, AssetConfig.Asset config)
        {
            int width = config.EffectiveWidth;
            int height = config.EffectiveHeight;
            if (height == 0)
                height = (int)streamLength / width;

            var sprite = new AlbionSprite { Name = name };
            // Debug.Assert(streamLength % width == 0);
            // Debug.Assert(streamLength % (width * height) == 0);

            long initialPosition = br.BaseStream.Position;
            int spriteCount = unchecked((int) (streamLength / (width * height)));
            height = (int) streamLength / (width * spriteCount);
            bool rotated = config.Parent.RotatedLeft;

            sprite.Width = width;
            sprite.Height = height * spriteCount;
            sprite.Frames = new AlbionSprite.Frame[spriteCount];
            sprite.UniformFrames = true;
            sprite.PixelData = new byte[spriteCount * width * height];

            int currentY = 0;
            for (int n = 0; n < spriteCount; n++)
            {
                sprite.Frames[n] = new AlbionSprite.Frame(0, currentY, width, height);

                var bytes = br.ReadBytes(width * height);
                if (rotated)
                {
                    for (int i = 0; i < width * height; i++)
                    {
                        int destX = width - (i / height) - 1;
                        int destY = currentY + i % height;
                        sprite.PixelData[destY * sprite.Width + destX] = bytes[i];
                    }
                }
                else
                {
                    for (int i = 0; i < width * height; i++)
                        sprite.PixelData[n * width * height + i] = bytes[i];
                }

                currentY += height;
            }

            // Debug.Assert(br.BaseStream.Position == initialPosition + streamLength);
            return sprite;
        }
    }
}