using System;
using SerdesNet;
using UAlbion.Api;
using UAlbion.Config;
using UAlbion.Formats.Assets;

namespace UAlbion.Formats.Parsers
{
    public class FixedSizeSpriteLoader : IAssetLoader<IEightBitImage>
    {
        public const string TypeString = "UAlbion.Formats.Parsers.FixedSizeSpriteLoader, UAlbion.Formats";

        public object Serdes(object existing, AssetInfo info, AssetMapping mapping, ISerializer s)
            => Serdes((IEightBitImage) existing, info, mapping, s);

        public IEightBitImage Serdes(IEightBitImage existing, AssetInfo info, AssetMapping mapping, ISerializer s)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));
            if (info == null) throw new ArgumentNullException(nameof(info));
            if (s.IsWriting() && existing == null) throw new ArgumentNullException(nameof(existing));
            // TODO: Assert uniform frames when writing

            var streamLength = s.IsWriting() && existing != null 
                ? existing.Width * existing.Height * existing.SubImageCount
                : s.BytesRemaining;

            if (streamLength == 0)
                return null;

            int width = existing?.Width ?? info.Width;
            int height = existing?.Height ?? info.Height;
            if (width == 0) width = (int)Math.Sqrt(streamLength);
            if (height == 0) height = (int)streamLength / width;

            // ApiUtil.Assert(streamLength % width == 0);
            // ApiUtil.Assert(streamLength % (width * height) == 0);

            // long initialPosition = s.BaseStream.Position;
            int spriteCount = Math.Max(1, unchecked((int)(streamLength / (width * height))));
            height = (int)streamLength / (width * spriteCount);

            byte[] pixelData = existing?.PixelData.ToArray();
            if (existing != null && info.Get(AssetProperty.Transposed, false))
            {
                pixelData = new byte[existing.PixelData.Length];
                for (int n = 0; n < spriteCount; n++)
                {
                    ApiUtil.TransposeImage(width, height,
                        existing.PixelData.Slice(n * width * height, width * height),
                        new Span<byte>(pixelData, n * width * height, width * height));
                }
            }

            pixelData = s.ByteArray(null, pixelData, width * height * spriteCount);
            if (existing != null)
                return existing;

            var frames = new AlbionSpriteFrame[spriteCount];
            for (int n = 0; n < spriteCount; n++)
                frames[n] ??= new AlbionSpriteFrame(0, height * n, width, height, width);

            var sprite = new AlbionSprite2(info.AssetId, width, height * spriteCount, true, pixelData, frames);
            if (!info.Get(AssetProperty.Transposed, false))
                return sprite;

            int rotatedFrameHeight = width;
            pixelData = new byte[spriteCount * width * height];
            frames = new AlbionSpriteFrame[spriteCount];
            for (int n = 0; n < spriteCount; n++)
            {
                frames[n] = new AlbionSpriteFrame(0, rotatedFrameHeight * n, height, rotatedFrameHeight, height);

                ApiUtil.TransposeImage(width, height,
                    new ReadOnlySpan<byte>(sprite.PixelData, n * width * height, width * height),
                    new Span<byte>(pixelData, n * width * height, width * height));
            }

            return new AlbionSprite2(info.AssetId, height, width * spriteCount, true, pixelData, frames);
        }
    }
}
