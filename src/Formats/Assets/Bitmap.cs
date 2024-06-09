using System;
using System.Collections.Generic;
using System.Text;
using SerdesNet;

namespace UAlbion.Formats.Assets;

public class Bitmap8Bit
{
    const string MagicString = "BM";
    static readonly byte[] Magic = Encoding.ASCII.GetBytes(MagicString);
    Bitmap8Bit() { } 
    public Bitmap8Bit(uint width, uint[] palette, byte[] pixels)
    {
        Palette = palette ?? throw new ArgumentNullException(nameof(palette));
        Pixels = pixels ?? throw new ArgumentNullException(nameof(pixels));
        if (Pixels.Length % width != 0)
            throw new ArgumentException($"Pixel array length ({pixels.Length}) is not an even multiple of the width ({width})");
        Width = width;
        Height = (uint)(Pixels.Length / width);
    }

    public uint Width { get; private set; }
    public uint Height { get; private set; }
    public IList<uint> Palette { get; private set; }
    public byte[] Pixels { get; private set; }

    public static Bitmap8Bit Serdes(Bitmap8Bit b, ISerializer s)
    {
        ArgumentNullException.ThrowIfNull(s);

        var initialOffset = s.Offset;
        b ??= new Bitmap8Bit();

        //-------------\\
        // File Header \\
        //-------------\\
        var magic = Encoding.ASCII.GetString(s.Bytes("Magic", Magic, 2)); // 0
        if (magic != MagicString)
            throw new FormatException($"Tried to read as bitmap, but incorrect signature \"{magic}\" found (expected BM)");

        var sizeOffset = s.Offset;
        uint size = s.UInt32("Size", 0); // 2 Will patch this at the end when writing
        if (s.IsReading() && size > s.BytesRemaining + (s.Offset - initialOffset))
            throw new FormatException($"Bitmap specified a size of {size}, but the stream only contains {s.BytesRemaining + (s.Offset - initialOffset)} bytes");

        s.UInt16("Reserved1", 0); // 6
        s.UInt16("Reserved2", 0); // 8
        var pixelOffsetOffset = s.Offset;
        uint pixelOffset = s.UInt32("PixelOffset", 0); // A Will patch later when writing
        if (s.IsReading() && pixelOffset > s.BytesRemaining + (s.Offset - initialOffset))
            throw new FormatException($"Bitmap specified a pixel offset of {pixelOffset}, but the stream only contains {s.BytesRemaining + (s.Offset - initialOffset)} bytes");

        //------------\\
        // DIB Header \\
        //------------\\
        const uint bitmapInfoHeaderSize = 40;
        uint headerSize = s.UInt32("HeaderSize", bitmapInfoHeaderSize); // E
        if (headerSize != bitmapInfoHeaderSize)
            throw new FormatException($"Unsupported bitmap header size {headerSize}, expecting {bitmapInfoHeaderSize}.");

        b.Width = s.UInt32("Width", b.Width); // 12
        b.Height = s.UInt32("Width", b.Height); // 16
        ushort planes = s.UInt16("Planes", 1); // 1A
        if (planes != 1)
            throw new FormatException($"Unsupported number of colour planes in bitmap: {planes}, expected 1");

        ushort bpp = s.UInt16("BPP", 8); // 1C
        if (bpp != 8)
            throw new FormatException($"Unsupported colour depth: {bpp} bits per pixel, expected 8");

        Compression compression = s.EnumU32("Compression", Compression.Rgb); // 1E
        if (compression != Compression.Rgb)
            throw new FormatException($"Unsupported compression method {compression}, expected RGB (0)");

        s.UInt32("ImageSize", 0); // 22 Dummy value for RGB bitmaps
        s.Int32("HorizontalResolution", 3779); // 26
        s.Int32("VerticalResolution", 3779); // 2A
        uint paletteSize = s.UInt32("PaletteSize", (uint)b.Palette.Count); // 2E
        if (paletteSize != 256)
            throw new FormatException($"Unsupported palette size {paletteSize}, expected 256");

        s.UInt32("ImportantColours", 0); // 32 Ignored

        //---------\\
        // Palette \\ // 36
        //---------\\
        b.Palette = s.List("Palette", b.Palette, (int)paletteSize, (_, x, s2) =>
        {
            uint b1 = s2.UInt8(null, (byte)((x & 0x00ff0000) >> 16)); // B
            uint b2 = s2.UInt8(null, (byte)((x & 0x0000ff00) >> 8));  // G
            uint b3 = s2.UInt8(null, (byte)(x & 0x000000ff));         // R
            s2.UInt8(null, 0);                                        // A
            return 0xff000000U | (b1 << 16) | (b2 << 8) | b3;
        });

        //------------\\
        // Pixel Data \\
        //------------\\
        if (s.IsReading())
        {
            var tempOffset = s.Offset;
            s.Seek(pixelOffsetOffset);
            s.UInt32("PixelOffset", (uint)(tempOffset - initialOffset));
            s.Seek(tempOffset);
        }

        b.Pixels ??= new byte[b.Width * b.Height];
        uint stride = 4 * ((bpp * b.Width + 31) / 32);
        for (int j = 0; j < b.Height; j++)
        {
            uint index = (uint)((b.Height - j - 1) * b.Width);
            for (int i = 0; i < b.Width; i++)
            {
                b.Pixels[index] = s.UInt8(null, b.Pixels[index]);
                index++;
            }

            if (stride - b.Width > 0) // Padding
                s.Pad((int)(stride - b.Width));
        }

        if (s.IsReading())
        {
            var tempOffset = s.Offset;
            s.Seek(sizeOffset);
            s.UInt32("Size", (uint)(tempOffset - initialOffset));
            s.Seek(tempOffset);
        }

        return b;
    }

    enum Compression : uint
    {
        Rgb = 0,
        /*
        Rle8 = 1,
        Rle4 = 2,
        BitFields = 3,
        Jpeg = 4,
        Png = 5,
        AlphaBitFields = 6,
        Cmyk = 11,
        CmykRle8 = 12,
        CmykRle4 = 13
        */
    }
}