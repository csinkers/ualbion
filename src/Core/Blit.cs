using System;
using System.Collections.Generic;
using UAlbion.Api;
using UAlbion.Api.Visual;

namespace UAlbion.Core
{
    public static class BlitUtil
    {
        static void Blit8To32Transparent(ReadOnlyByteImageBuffer fromBuffer, UIntImageBuffer toBuffer, uint[] palette, byte componentAlpha, byte transparentColor)
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

        static void Blit8To32Opaque(ReadOnlyByteImageBuffer fromBuffer, UIntImageBuffer toBuffer, uint[] palette, byte componentAlpha)
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

        public static void Blit8To32(ReadOnlyByteImageBuffer from, UIntImageBuffer to, uint[] palette, byte componentAlpha, byte? transparentColor)
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
                    var newFrom = new ReadOnlyByteImageBuffer(chunkWidth, chunkHeight, from.Stride, from.Buffer);
                    var newTo = new UIntImageBuffer(chunkWidth, chunkHeight, to.Stride, dest);

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

        public static void Blit32To8(ReadOnlyUIntImageBuffer fromBuffer, ByteImageBuffer toBuffer, uint[] palette, Dictionary<uint, byte> quantizeCache = null)
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

        public static IList<byte> DistinctColors(ReadOnlyByteImageBuffer buffer)
        {
            int c = 0;
            bool[] active = new bool[256];
            while (c < buffer.Buffer.Length)
            {
                int end = c + buffer.Width;
                while (c < end)
                {
                    active[buffer.Buffer[c]] = true;
                    c++;
                }

                c += buffer.Stride - buffer.Width;
            }

            var results = new List<byte>();
            for (int i = 0; i < 256; i++)
                if (active[i])
                    results.Add((byte)i);
            return results;
        }

    }
}