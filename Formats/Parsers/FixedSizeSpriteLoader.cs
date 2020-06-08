using System;
using System.IO;
using UAlbion.Api;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Config;

namespace UAlbion.Formats.Parsers
{
    [AssetLoader(FileFormat.FixedSizeSprite)]
    public class FixedSizeSpriteLoader : IAssetLoader
    {
        public object Load(BinaryReader br, long streamLength, AssetKey key, AssetInfo config)
        {
            if (streamLength == 0)
                return null;

            int width = config.EffectiveWidth;
            int height = config.EffectiveHeight;
            if (width == 0)
                width = (int) Math.Sqrt(streamLength);
            if (height == 0)
                height = (int)streamLength / width;

            // ApiUtil.Assert(streamLength % width == 0);
            // ApiUtil.Assert(streamLength % (width * height) == 0);

            // long initialPosition = br.BaseStream.Position;
            int spriteCount = Math.Max(1, unchecked((int)(streamLength / (width * height))));
            height = (int) streamLength / (width * spriteCount);

            var sprite = new AlbionSprite
            {
                Name = key.ToString(),
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

            if (config.Transposed)
            {
                var rotatedSprite = new AlbionSprite
                {
                    Name = key.ToString(),
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

                    ApiUtil.RotateImage(width, height,
                        new Span<byte>(sprite.PixelData, n * width * height, width * height),
                        new Span<byte>(rotatedSprite.PixelData, n * width * height, width * height));
                }

                return rotatedSprite;
            }

            // ApiUtil.Assert(br.BaseStream.Position == initialPosition + streamLength);
            return sprite;
        }
    }
}
