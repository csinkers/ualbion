using System;
using System.Buffers;
using SerdesNet;
using UAlbion.Api;
using UAlbion.Api.Visual;
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
            return s.IsWriting()
                ? Write(existing, info, s)
                : Read(info, s);
        }

        static IEightBitImage Read(AssetInfo info, ISerializer s)
        {
            var streamLength = s.BytesRemaining;
            if (streamLength == 0)
                return null;

            int width = info.Width;
            int height = info.Height;
            if (width == 0) width = (int)Math.Sqrt(streamLength);
            if (height == 0) height = (int)streamLength / width;

            int spriteCount = Math.Max(1, unchecked((int)(streamLength / (width * height))));
            height = (int)streamLength / (width * spriteCount);

            byte[] pixelData = s.Bytes(null, null, (int)streamLength);

            var frames = new AlbionSpriteFrame[spriteCount];
            for (int n = 0; n < spriteCount; n++)
                frames[n] = new AlbionSpriteFrame(0, height * n, width, height, width);

            var sprite = new AlbionSprite(info.AssetId, width, height * spriteCount, true, pixelData, frames);
            return info.Get(AssetProperty.Transposed, false) ? Transpose(sprite) : sprite;
        }

        static IEightBitImage Write(IEightBitImage existing, AssetInfo info, ISerializer s)
        {
            if (existing == null) throw new ArgumentNullException(nameof(existing));

            var f = existing.GetSubImage(0);
            for (int i = 0; i < existing.SubImageCount; i++)
            {
                var frame = existing.GetSubImage(i);
                ApiUtil.Assert(f.Width == frame.Width, "FixedSizeSpriteLoader tried to serialise sprite with non-uniform frames");
                ApiUtil.Assert(f.Height == frame.Height, "FixedSizeSpriteLoader tried to serialise sprite with non-uniform frames");
            }

            var sprite = info.Get(AssetProperty.Transposed, false)
                ? Transpose(existing)
                : existing;

            InnerWrite(sprite, s);
            return existing;
        }

        static void InnerWrite(IEightBitImage sprite, ISerializer s)
        {
            var f = sprite.GetSubImage(0);
            int frameSize = f.Width * f.Height;
            byte[] pixelData = ArrayPool<byte>.Shared.Rent(frameSize);
            try
            {
                for (int i = 0; i < sprite.SubImageCount; i++)
                {
                    var frame = sprite.GetSubImage(i);
                    FormatUtil.Blit(
                        sprite.PixelData.AsSpan(frame.PixelOffset, frame.PixelLength),
                        pixelData.AsSpan(),
                        f.Width, f.Height,
                        sprite.Width, f.Width);
                    s.Bytes(null, pixelData, frameSize);
                }
            }
            finally { ArrayPool<byte>.Shared.Return(pixelData); }
        }

        static IEightBitImage Transpose(IEightBitImage sprite)
        {
            var firstFrame = sprite.GetSubImage(0);
            int width = firstFrame.Width;
            int height = firstFrame.Height;
            int spriteCount = sprite.SubImageCount;

            int rotatedFrameHeight = width;
            byte[] pixelData = new byte[spriteCount * width * height];
            var frames = new AlbionSpriteFrame[spriteCount];
            for (int i = 0; i < spriteCount; i++)
            {
                var oldFrame = sprite.GetSubImage(i);
                frames[i] = new AlbionSpriteFrame(0, rotatedFrameHeight * i, height, rotatedFrameHeight, height);

                ApiUtil.TransposeImage(width, height, // TODO: This should really take stride via ImageBuffers etc
                    sprite.PixelData.AsSpan(oldFrame.PixelOffset, oldFrame.PixelLength),
                    pixelData.AsSpan(frames[i].PixelOffset, frames[i].PixelLength));
            }
            return new AlbionSprite(AssetId.FromUInt32(sprite.Id.ToUInt32()), height, width * spriteCount, true, pixelData, frames);
        }
    }
}
