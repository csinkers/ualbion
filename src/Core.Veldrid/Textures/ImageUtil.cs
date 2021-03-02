using System;
using System.Runtime.InteropServices;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using UAlbion.Api;
using UAlbion.Core.Visual;

namespace UAlbion.Core.Veldrid.Textures
{
    public static class ImageUtil
    {
        public static Image<Rgba32> BuildImageForFrame(ReadOnlyByteImageBuffer from, uint[] palette)
        {
            Image<Rgba32> image = new Image<Rgba32>((int)from.Width, (int)from.Height);
            if (!image.TryGetSinglePixelSpan(out var rgbaSpan))
                throw new InvalidOperationException("Could not retrieve single span from Image");

            Span<uint> toBuffer = MemoryMarshal.Cast<Rgba32, uint>(rgbaSpan);
            var to = new UIntImageBuffer(from.Width, from.Height, (int)from.Width, toBuffer);
            CoreUtil.Blit8To32(from, to, palette, 255, 0);

            return image;
        }

        public delegate ReadOnlyByteImageBuffer GetByteFrameDelegate(int frame);
        public static Image<Rgba32> PackSpriteSheet(uint[] palette, int frameCount, GetByteFrameDelegate getFrame)
        {
            long totalPixels = 0;
            int width = 0;

            for(int i = 0; i < frameCount; i++)
            {
                var frame = getFrame(i);
                totalPixels += frame.Width * frame.Height;
                if (width < frame.Width) width = (int)frame.Width;
            }

            int sqrtTotal = (int)Math.Sqrt(totalPixels);
            if (sqrtTotal > width)
                width = sqrtTotal;
            width = ApiUtil.NextPowerOfTwo(width);

            // First arrange to determine required size and positions, then create the image.
            var positions = new (int, int)[frameCount];
            int rowHeight = 0;
            int curX = 0, curY = 0;
            for (var index = 0; index < frameCount; index++)
            {
                var si = getFrame(index);
                int w = (int)si.Width;
                int h = (int)si.Height;

                if (width - (curX + w) >= 0) // Still room left on this row
                {
                    positions[index] = (curX, curY);
                    curX += w;
                    if (h > rowHeight)
                        rowHeight = h;
                }
                else // Start a new row
                {
                    curY += rowHeight;
                    rowHeight = h;
                    positions[index] = (0, curY);
                    curX = w;
                }
            }

            if (curX > 0)
                curY += rowHeight;

            var height = curY;

            Image<Rgba32> image = new Image<Rgba32>(width, height);
            if (!image.TryGetSinglePixelSpan(out var rgbaSpan))
                throw new InvalidOperationException("Could not retrieve single span from Image");

            Span<uint> pixels = MemoryMarshal.Cast<Rgba32, uint>(rgbaSpan);
            for (var index = 0; index < frameCount; index++)
            {
                var frame = getFrame(index);
                var (toX, toY) = positions[index];
                var target = pixels.Slice(toX + toY * width);
                var to = new UIntImageBuffer(frame.Width, frame.Height, width, target);
                CoreUtil.Blit8To32(frame, to, palette, 255, 0);
            }

            return image;
        }

        public static void UnpackSpriteSheet(uint[] palette, int frameWidth, int frameHeight, ReadOnlyUIntImageBuffer sheet, ByteImageBuffer dest, Action<int, int, int, int> frameFunc)
        {
            if (dest.Width < sheet.Width) throw new ArgumentOutOfRangeException(nameof(dest), "Tried to unpack to a smaller destination");
            if (dest.Height < sheet.Height) throw new ArgumentOutOfRangeException(nameof(dest), "Tried to unpack to a smaller destination");

            CoreUtil.Blit32To8(sheet, dest, palette);

            int x = 0; int y = 0;
            do
            {
                frameFunc(x, y, frameWidth, frameHeight);
                x += frameWidth;
                if (x + frameWidth > sheet.Width)
                {
                    y += frameHeight;
                    x = 0;
                }
            } while (y + frameHeight <= sheet.Height);
        }
    }
}