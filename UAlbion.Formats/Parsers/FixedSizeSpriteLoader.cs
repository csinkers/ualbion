using System.IO;

namespace UAlbion.Formats.Parsers
{
    [AssetLoader(XldObjectType.FixedSizeSprite)]
    public class FixedSizeSpriteLoader : IAssetLoader
    {
        public object Load(BinaryReader br, long streamLength, string name, AssetConfig.Asset config)
        {
            if (streamLength == 0)
                return null;

            int width = config.EffectiveWidth;
            int height = config.EffectiveHeight;
            if (height == 0)
                height = (int)streamLength / width;

            // Debug.Assert(streamLength % width == 0);
            // Debug.Assert(streamLength % (width * height) == 0);

            long initialPosition = br.BaseStream.Position;
            int spriteCount = unchecked((int) (streamLength / (width * height)));
            height = (int) streamLength / (width * spriteCount);

            var sprite = new AlbionSprite
            {
                Name = name,
                Width = width,
                Height = height * spriteCount,
                Frames = new AlbionSprite.Frame[spriteCount],
                UniformFrames = true,
                PixelData = new byte[spriteCount * width * height]
            };

            int currentY = 0;
            for (int n = 0; n < spriteCount; n++)
            {
                sprite.Frames[n] = new AlbionSprite.Frame(0, currentY, width, height);

                var bytes = br.ReadBytes(width * height);
                for (int i = 0; i < width * height; i++)
                    sprite.PixelData[n * width * height + i] = bytes[i];

                currentY += height;
            }

            if (config.Parent.RotatedLeft)
            {
                var rotatedSprite = new AlbionSprite
                {
                    Name = name,
                    Width = height,
                    Height = width * spriteCount,
                    Frames = new AlbionSprite.Frame[spriteCount],
                    UniformFrames = true,
                    PixelData = new byte[spriteCount * width * height]
                };
                int rotatedFrameHeight = width;

                for (int n = 0; n < spriteCount; n++)
                {
                    rotatedSprite.Frames[n] = new AlbionSprite.Frame(
                        0, rotatedFrameHeight * n, 
                        rotatedSprite.Width, rotatedFrameHeight);
                    int x = rotatedSprite.Width - 1;
                    int y = 0;
                    for (int i = 0; i < width * height; i++)
                    {
                        int sourceIndex = n * width * height + i;
                        int destIndex = y * rotatedSprite.Width + x + n * width * height;
                        rotatedSprite.PixelData[destIndex] = sprite.PixelData[sourceIndex];

                        y++;
                        if (y == rotatedFrameHeight)
                        {
                            y = 0;
                            x--;
                        }
                    }
                }

                return rotatedSprite;
            }

            // Debug.Assert(br.BaseStream.Position == initialPosition + streamLength);
            return sprite;
        }
    }
}