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
            if (br == null) throw new ArgumentNullException(nameof(br));
            if (config == null) throw new ArgumentNullException(nameof(config));

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

            int currentY = 0;
            var pixelData = new byte[spriteCount * width * height];
            var frames = new AlbionSpriteFrame[spriteCount];

            for (int n = 0; n < spriteCount; n++)
            {
                frames[n] = new AlbionSpriteFrame(0, currentY, width, height);

                var bytes = br.ReadBytes(width * height);
                for (int i = 0; i < width * height; i++)
                    pixelData[n * width * height + i] = bytes[i];

                currentY += height;
            }

            var sprite = new AlbionSprite(key.ToString(), width, height * spriteCount, true, pixelData, frames);
            if (!config.Transposed)
                return sprite;

            int rotatedFrameHeight = width;
            pixelData = new byte[spriteCount * width * height];
            frames = new AlbionSpriteFrame[spriteCount];
            for (int n = 0; n < spriteCount; n++)
            {
                frames[n] = new AlbionSpriteFrame(0, rotatedFrameHeight * n, height, rotatedFrameHeight);

                ApiUtil.RotateImage(width, height,
                    new ReadOnlySpan<byte>(sprite.PixelData, n * width * height, width * height),
                    new Span<byte>(pixelData, n * width * height, width * height));
            }

            return new AlbionSprite(key.ToString(), height, width * spriteCount, true, pixelData, frames);
        }
    }
}
