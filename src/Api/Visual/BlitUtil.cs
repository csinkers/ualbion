using System;
using System.Collections.Generic;
using System.Linq;

namespace UAlbion.Api.Visual;

public static class BlitUtil
{
    static void Blit8To32Transparent(ReadOnlyImageBuffer<byte> fromBuffer, ImageBuffer<uint> toBuffer, uint[] palette, byte componentAlpha, byte transparentColor)
    {
        var from = fromBuffer.Buffer;
        var to = toBuffer.Buffer;
        int fromOffset = 0;
        int toOffset = 0;

        for (int j = 0; j < fromBuffer.Height; j++)
        {
            for (int i = 0; i < fromBuffer.Width; i++)
            {
                byte index = from[fromOffset];
                if (index != transparentColor)
                    to[toOffset] = palette[index] & 0x00ffffff | ((uint)componentAlpha << 24);

                fromOffset++;
                toOffset++;
            }

            fromOffset += fromBuffer.Stride - fromBuffer.Width;
            toOffset += toBuffer.Stride - toBuffer.Width;
        }
    }

    static void Blit8To32Opaque(ReadOnlyImageBuffer<byte> fromBuffer, ImageBuffer<uint> toBuffer, uint[] palette, byte componentAlpha)
    {
        var from = fromBuffer.Buffer;
        var to = toBuffer.Buffer;
        int fromOffset = 0;
        int toOffset = 0;

        for (int j = 0; j < fromBuffer.Height; j++)
        {
            for (int i = 0; i < fromBuffer.Width; i++)
            {
                byte index = from[fromOffset];
                uint color = palette[index] & 0x00ffffff | ((uint)componentAlpha << 24);
                to[toOffset] = color;
                fromOffset++;
                toOffset++;
            }

            fromOffset += fromBuffer.Stride - fromBuffer.Width;
            toOffset += toBuffer.Stride - toBuffer.Width;
        }
    }

    public static void BlitTiled8To32(ReadOnlyImageBuffer<byte> from, ImageBuffer<uint> to, uint[] palette, byte componentAlpha, byte? transparentColor)
    {
        if (palette == null) throw new ArgumentNullException(nameof(palette));
        int remainingWidth = to.Width;
        int remainingHeight = to.Height;
        Span<uint> dest = to.Buffer;

        int chunkHeight = Math.Min(from.Height, to.Height);
        do
        {
            Span<uint> rowStart = dest;
            chunkHeight = Math.Min(chunkHeight, remainingHeight);
            int chunkWidth = Math.Min(from.Width, to.Width);
            do
            {
                chunkWidth = Math.Min(chunkWidth, remainingWidth);
                var newFrom = new ReadOnlyImageBuffer<byte>(chunkWidth, chunkHeight, from.Stride, from.Buffer);
                var newTo = new ImageBuffer<uint>(chunkWidth, chunkHeight, to.Stride, dest);

                if (transparentColor.HasValue)
                    Blit8To32Transparent(newFrom, newTo, palette, componentAlpha, transparentColor.Value);
                else
                    Blit8To32Opaque(newFrom, newTo, palette, componentAlpha);

                dest = dest.Slice(chunkWidth);
                remainingWidth -= chunkWidth;
            } while (remainingWidth > 0);

            remainingHeight -= chunkHeight;
            remainingWidth = to.Width;
            if (remainingHeight > 0)
                dest = rowStart.Slice(chunkHeight * to.Stride);
        } while (remainingHeight > 0);
    }

    static byte Quantize(uint value, uint[] palette)
    {
        if (palette == null) throw new ArgumentNullException(nameof(palette));
        if (palette.Length > 256) throw new ArgumentOutOfRangeException(nameof(palette), "Only 8-bit palettes are supported");

        var (r, g, b, a) = ApiUtil.UnpackColor(value);

        byte result = 0;
        int best = int.MaxValue;
        for (int i = 0; i < palette.Length; i++)
        {
            var (r2, g2, b2, a2) = ApiUtil.UnpackColor(palette[i]);
            int dr = r - r2;
            int dg = g - g2;
            int db = b - b2;
            int da = a - a2;
            int dist2 = dr * dr + dg * dg + db * db + da * da;
            if (dist2 < best)
            {
                best = dist2;
                result = (byte)i;
            }
        }
        return result;
    }

    public static void Blit32To8(ReadOnlyImageBuffer<uint> fromBuffer, ImageBuffer<byte> toBuffer, uint[] palette, Dictionary<uint, byte> quantizeCache = null)
    {
        quantizeCache ??= new Dictionary<uint, byte>();
        var from = fromBuffer.Buffer;
        var to = toBuffer.Buffer;
        int fromOffset = 0;
        int toOffset = 0;

        for (int j = 0; j < fromBuffer.Height; j++)
        {
            for (int i = 0; i < fromBuffer.Width; i++)
            {
                uint pixel = from[fromOffset];
                if (!quantizeCache.TryGetValue(pixel, out var index))
                {
                    index = Quantize(pixel, palette);
                    quantizeCache[pixel] = index;
                }

                to[toOffset] = index;

                fromOffset++;
                toOffset++;
            }

            fromOffset += fromBuffer.Stride - fromBuffer.Width;
            toOffset += toBuffer.Stride - toBuffer.Width;
        }
    }

    public static bool OpacityFunc8(byte pixel) => pixel != 0;
    public static bool OpacityFunc32(uint pixel) => (byte)((pixel >> 24) & 0xff) > 0;

    public static void BlitDirect<T>(ReadOnlyImageBuffer<T> fromBuffer, ImageBuffer<T> toBuffer) where T : unmanaged
    {
        for (int j = 0; j < fromBuffer.Height; j++)
        {
            var fromSlice = fromBuffer.Buffer.Slice(j * fromBuffer.Stride, fromBuffer.Width);
            var toSlice = toBuffer.Buffer.Slice(j * toBuffer.Stride, toBuffer.Width);
            fromSlice.CopyTo(toSlice);
        }
    }

    public static void BlitMasked<T>(ReadOnlyImageBuffer<T> fromBuffer, ImageBuffer<T> toBuffer, Func<T, bool> opacityFunc) where T : unmanaged
    {
        var from = fromBuffer.Buffer;
        var to = toBuffer.Buffer;
        int fromOffset = 0;
        int toOffset = 0;

        for (int j = 0; j < fromBuffer.Height; j++)
        {
            for (int i = 0; i < fromBuffer.Width; i++)
            {
                var pixel = from[fromOffset];
                if (opacityFunc(pixel))
                    to[toOffset] = pixel;

                fromOffset++;
                toOffset++;
            }

            fromOffset += fromBuffer.Stride - fromBuffer.Width;
            toOffset += toBuffer.Stride - toBuffer.Width;
        }
    }

    public static void BlitTiled<T>(ReadOnlyImageBuffer<T> from, ImageBuffer<T> to, Func<T, bool> opacityFunc = null) where T : unmanaged
    {
        int remainingWidth = to.Width;
        int remainingHeight = to.Height;
        Span<T> dest = to.Buffer;

        int chunkHeight = Math.Min(from.Height, to.Height);
        do
        {
            Span<T> rowStart = dest;
            chunkHeight = Math.Min(chunkHeight, remainingHeight);
            int chunkWidth = Math.Min(from.Width, to.Width);
            do
            {
                chunkWidth = Math.Min(chunkWidth, remainingWidth);
                var newFrom = new ReadOnlyImageBuffer<T>(chunkWidth, chunkHeight, from.Stride, from.Buffer);
                var newTo = new ImageBuffer<T>(chunkWidth, chunkHeight, to.Stride, dest);

                if (opacityFunc == null)
                    BlitDirect(newFrom, newTo);
                else
                    BlitMasked(newFrom, newTo, opacityFunc);

                dest = dest.Slice(chunkWidth);
                remainingWidth -= chunkWidth;
            } while (remainingWidth > 0);

            remainingHeight -= chunkHeight;
            remainingWidth = to.Width;
            if (remainingHeight > 0)
                dest = rowStart.Slice(chunkHeight * to.Stride);
        } while (remainingHeight > 0);
    }

    public static void BlitTiled8(ReadOnlyImageBuffer<byte> from, ImageBuffer<byte> to) => BlitTiled(from, to, OpacityFunc8);
    public static void BlitTiled32(ReadOnlyImageBuffer<uint> from, ImageBuffer<uint> to) => BlitTiled(from, to, OpacityFunc32);

    /*
    public static void Blit8(ReadOnlySpan<byte> from, Span<byte> to, int width, int height, int fromStride, int toStride)
    {
        int srcIndex = 0;
        int destIndex = 0;
        for (int i = 0; i < height; i++)
        {
            var row = from.Slice(srcIndex, width);
            row.CopyTo(to.Slice(destIndex));
            srcIndex += fromStride;
            destIndex += toStride;
        }
    } */


    public static ISet<T> DistinctColors<T>(ReadOnlyImageBuffer<T> buffer) where T : unmanaged
    {
        int c = 0;
        var active = new HashSet<T>();
        while (c < buffer.Buffer.Length)
        {
            int end = c + buffer.Width;
            while (c < end)
            {
                active.Add(buffer.Buffer[c]);
                c++;
            }

            c += buffer.Stride - buffer.Width;
        }

        return active;
    }

    public static ISet<T> DistinctColors<T>(ReadOnlySpan<T> buffer) where T : unmanaged
    {
        var active = new HashSet<T>();
        foreach (var pixel in buffer)
            active.Add(pixel);
        return active;
    }

    public static void UnpackSpriteSheet(
        uint[] palette,
        int frameWidth,
        int frameHeight,
        ReadOnlyImageBuffer<uint> source,
        ImageBuffer<byte> dest,
        Action<int, int, int, int> frameFunc)
    {
        if (dest.Width < source.Width) throw new ArgumentOutOfRangeException(nameof(dest), "Tried to unpack to a smaller destination");
        if (dest.Height < source.Height) throw new ArgumentOutOfRangeException(nameof(dest), "Tried to unpack to a smaller destination");

        Blit32To8(source, dest, palette);

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

    public static long CalculatePalettePeriod(ISet<byte> colors, IPalette palette)
    {
        if (colors == null) throw new ArgumentNullException(nameof(colors));
        if (palette == null) throw new ArgumentNullException(nameof(palette));

        var periods =
            palette.AnimatedEntries
                .Where(x => colors.Contains(x.Item1))
                .Select(x => (long)x.Item2)
                .Distinct();

        return ApiUtil.Lcm(periods);
    }
}