using System;
using System.Buffers;
using SerdesNet;
using UAlbion.Api;
using UAlbion.Api.Visual;
using UAlbion.Config;

namespace UAlbion.Formats.Parsers
{
    public class FixedSizeSpriteLoader : IAssetLoader<IReadOnlyTexture<byte>>
    {
        public const string TypeString = "UAlbion.Formats.Parsers.FixedSizeSpriteLoader, UAlbion.Formats";

        public object Serdes(object existing, AssetInfo info, AssetMapping mapping, ISerializer s)
            => Serdes((IReadOnlyTexture<byte>) existing, info, mapping, s);

        public IReadOnlyTexture<byte> Serdes(IReadOnlyTexture<byte> existing, AssetInfo info, AssetMapping mapping, ISerializer s)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));
            if (info == null) throw new ArgumentNullException(nameof(info));
            return s.IsWriting()
                ? Write(existing, info, s)
                : Read(info, s);
        }

        static IReadOnlyTexture<byte> Read(AssetInfo info, ISerializer s)
        {
            var streamLength = s.BytesRemaining;
            if (streamLength == 0)
                return null;

            int width = info.Width;
            int height = info.Height;

            if (width == 0) width = (int)Math.Sqrt(streamLength);

            int totalHeight = (int)streamLength / width;
            if (height == 0) height = totalHeight;

            int spriteCount = Math.Max(1, unchecked((int)(streamLength / (width * height))));
            height = (int)streamLength / (width * spriteCount);

            byte[] pixelData = s.Bytes(null, null, (int)streamLength);
            int expectedPixelCount = width * height * spriteCount;
            ApiUtil.Assert(expectedPixelCount == (int)streamLength,
                $"Extra pixels found when loading fixed size sprite {info.AssetId} " +
                $"({streamLength} bytes for a {width}x{height}x{spriteCount} image, expected {expectedPixelCount}");

            var frames = new Region[spriteCount];
            for (int n = 0; n < spriteCount; n++)
                frames[n] = new Region(0, height * n, width, height, width, totalHeight, 0);

            var sprite = new SimpleTexture<byte>(
                info.AssetId,
                info.AssetId.ToString(),
                width,
                height * spriteCount,
                pixelData.AsSpan(0, expectedPixelCount), // May be less than the streamlength
                frames);

            return info.Get(AssetProperty.Transposed, false) ? Transpose(sprite) : sprite;
        }

        static IReadOnlyTexture<byte> Write(IReadOnlyTexture<byte> existing, AssetInfo info, ISerializer s)
        {
            if (existing == null) throw new ArgumentNullException(nameof(existing));

            var f = existing.Regions[0];
            foreach (var frame in existing.Regions)
            {
                ApiUtil.Assert(f.Width == frame.Width, "FixedSizeSpriteLoader tried to serialise sprite with non-uniform frames");
                ApiUtil.Assert(f.Height == frame.Height, "FixedSizeSpriteLoader tried to serialise sprite with non-uniform frames");
            }

            var sprite = info.Get(AssetProperty.Transposed, false)
                ? Transpose(existing)
                : existing;

            InnerWrite(sprite, s);
            return existing;
        }

        static void InnerWrite(IReadOnlyTexture<byte> sprite, ISerializer s)
        {
            var f = sprite.Regions[0];
            int frameSize = f.Width * f.Height;
            byte[] pixelData = ArrayPool<byte>.Shared.Rent(frameSize);
            try
            {
                for (int i = 0; i < sprite.Regions.Count; i++)
                {
                    BlitUtil.BlitDirect(
                        sprite.GetRegionBuffer(i),
                        new ImageBuffer<byte>(f.Width, f.Height, f.Width, pixelData));

                    s.Bytes(null, pixelData, frameSize);
                }
            }
            finally { ArrayPool<byte>.Shared.Return(pixelData); }
        }

        static IReadOnlyTexture<byte> Transpose(IReadOnlyTexture<byte> sprite)
        {
            var firstFrame = sprite.Regions[0];
            int width = firstFrame.Width;
            int height = firstFrame.Height;
            int spriteCount = sprite.Regions.Count;

            int rotatedFrameHeight = width;
            byte[] pixelData = new byte[spriteCount * width * height];
            var frames = new Region[spriteCount];
            for (int i = 0; i < spriteCount; i++)
            {
                var oldFrame = sprite.Regions[i];
                frames[i] = new Region(0, rotatedFrameHeight * i, height, rotatedFrameHeight, height, width * spriteCount, 0);

                ApiUtil.TransposeImage(width, height, // TODO: This should really take stride via ImageBuffers etc
                    sprite.PixelData.Slice(oldFrame.PixelOffset, oldFrame.PixelLength),
                    pixelData.AsSpan(frames[i].PixelOffset, frames[i].PixelLength));
            }
            return new SimpleTexture<byte>(sprite.Id, sprite.Name, height, width * spriteCount, pixelData, frames);
        }
    }
}
