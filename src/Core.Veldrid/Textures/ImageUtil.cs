using System;
using System.Runtime.InteropServices;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using UAlbion.Api.Visual;

namespace UAlbion.Core.Veldrid.Textures
{
    public static class ImageUtil
    {
        public static Image<Rgba32> BuildImageForFrame(ReadOnlyByteImageBuffer from, uint[] palette)
        {
            Image<Rgba32> image = new Image<Rgba32>(from.Width, from.Height);
            if (!image.TryGetSinglePixelSpan(out var rgbaSpan))
                throw new InvalidOperationException("Could not retrieve single span from Image");

            Span<uint> toBuffer = MemoryMarshal.Cast<Rgba32, uint>(rgbaSpan);
            var to = new UIntImageBuffer(from.Width, from.Height, from.Width, toBuffer);
            BlitUtil.Blit8To32(from, to, palette, 255, 0);

            return image;
        }

        public static Image<Rgba32> PackSpriteSheet(uint[] palette, int frameCount, GetByteFrameDelegate getFrame)
        {
            var layout = SpriteSheetUtil.ArrangeSpriteSheet(frameCount, 0, getFrame);
            Image<Rgba32> image = new Image<Rgba32>(layout.Width, layout.Height);
            if (!image.TryGetSinglePixelSpan(out var rgbaSpan))
                throw new InvalidOperationException("Could not retrieve single span from Image");

            Span<uint> pixels = MemoryMarshal.Cast<Rgba32, uint>(rgbaSpan);
            for (var i = 0; i < frameCount; i++)
            {
                var frame = getFrame(i);
                var (toX, toY) = layout.Positions[i];
                var target = pixels.Slice(toX + toY * layout.Width);
                var to = new UIntImageBuffer(frame.Width, frame.Height, layout.Width, target);
                BlitUtil.Blit8To32(frame, to, palette, 255, 0);
            }

            return image;
        }

        public static void UnpackSpriteSheet(
            uint[] palette,
            int frameWidth,
            int frameHeight,
            ReadOnlyUIntImageBuffer source,
            ByteImageBuffer dest,
            Action<int, int, int, int> frameFunc)
        {
            if (dest.Width < source.Width) throw new ArgumentOutOfRangeException(nameof(dest), "Tried to unpack to a smaller destination");
            if (dest.Height < source.Height) throw new ArgumentOutOfRangeException(nameof(dest), "Tried to unpack to a smaller destination");

            BlitUtil.Blit32To8(source, dest, palette);

            int x = 0; int y = 0;
            do
            {
                frameFunc(x, y, frameWidth, frameHeight);
                x += frameWidth;
                if (x + frameWidth > source.Width)
                {
                    y += frameHeight;
                    x = 0;
                }
            } while (y + frameHeight <= source.Height);
        }
    }
}