using System;
using SerdesNet;
using UAlbion.Api;
using UAlbion.Config;
using UAlbion.Formats.Assets;

namespace UAlbion.Formats.Parsers
{
    public class FixedSizeSpriteLoader : IAssetLoader<AlbionSprite>
    {
        public const string TypeString = "UAlbion.Formats.Parsers.FixedSizeSpriteLoader, UAlbion.Formats";

        public object Serdes(object existing, AssetInfo config, AssetMapping mapping, ISerializer s)
            => Serdes((AlbionSprite) existing, config, mapping, s);

        public AlbionSprite Serdes(AlbionSprite existing, AssetInfo config, AssetMapping mapping, ISerializer s)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));
            if (config == null) throw new ArgumentNullException(nameof(config));
            if (s.IsWriting() && existing == null) throw new ArgumentNullException(nameof(existing));
            // TODO: Assert uniform frames when writing

            var streamLength = s.IsWriting() && existing != null 
                ? existing.Width * existing.Height * existing.Frames.Count 
                : s.BytesRemaining;

            if (streamLength == 0)
                return null;

            int width = existing?.Width ?? config.Width;
            int height = existing?.Height ?? config.Height;
            if (width == 0) width = (int)Math.Sqrt(streamLength);
            if (height == 0) height = (int)streamLength / width;

            // ApiUtil.Assert(streamLength % width == 0);
            // ApiUtil.Assert(streamLength % (width * height) == 0);

            // long initialPosition = s.BaseStream.Position;
            int spriteCount = Math.Max(1, unchecked((int)(streamLength / (width * height))));
            height = (int)streamLength / (width * spriteCount);

            byte[] pixelData = existing?.PixelData;
            if (existing != null && config.Transposed)
            {
                pixelData = new byte[existing.PixelData.Length];
                for (int n = 0; n < spriteCount; n++)
                {
                    ApiUtil.TransposeImage(width, height,
                        new ReadOnlySpan<byte>(existing.PixelData, n * width * height, width * height),
                        new Span<byte>(pixelData, n * width * height, width * height));
                }
            }

            pixelData = s.ByteArray(null, pixelData, width * height * spriteCount);
            if (existing != null)
                return existing;

            var frames = new AlbionSpriteFrame[spriteCount];
            for (int n = 0; n < spriteCount; n++)
                frames[n] ??= new AlbionSpriteFrame(0, height * n, width, height);

            var sprite = new AlbionSprite(config.AssetId.ToString(), width, height * spriteCount, true, pixelData, frames);
            if (!config.Transposed)
                return sprite;

            int rotatedFrameHeight = width;
            pixelData = new byte[spriteCount * width * height];
            frames = new AlbionSpriteFrame[spriteCount];
            for (int n = 0; n < spriteCount; n++)
            {
                frames[n] = new AlbionSpriteFrame(0, rotatedFrameHeight * n, height, rotatedFrameHeight);

                ApiUtil.TransposeImage(width, height,
                    new ReadOnlySpan<byte>(sprite.PixelData, n * width * height, width * height),
                    new Span<byte>(pixelData, n * width * height, width * height));
            }

            return new AlbionSprite(config.AssetId.ToString(), height, width * spriteCount, true, pixelData, frames);
        }
    }
}
