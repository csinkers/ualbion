using System;
using System.Collections.Generic;
using System.IO;
using SerdesNet;

namespace UAlbion.Formats.Assets;

public sealed class InterlacedBitmap
{
    public short Width { get; set; }
    public short Height { get; set; }
    public short PosX { get; set; }
    public short PosY { get; set; }
    public byte NumPlanes { get; set; }
    public byte Mask { get; set; }
    public byte Compression { get; set; }
    public byte Padding { get; set; }
    public short Transparent { get; set; }
    public short AspectRatio { get; set; }
    public short PageWidth { get; set; }
    public short PageHeight { get; set; }
    public uint[] Palette { get; set; }
    public short HotspotX { get; set; }
    public short HotspotY { get; set; }
    public IList<ColorRange> ColorRanges { get; private set; }
    public ushort ThumbnailWidth { get; set; }
    public ushort ThumbnailHeight { get; set; }
    public byte[] Thumbnail { get; set; }
    public byte[] ImageData { get; set; }

    public static InterlacedBitmap Serdes(InterlacedBitmap img, ISerializer s)
    {
        ArgumentNullException.ThrowIfNull(s);
        img ??= new InterlacedBitmap();

        var formatChunk = IffChunk.Serdes(0, new IffChunk(IffChunkType.Format, 0), s);
        if (formatChunk.TypeId != IffChunkType.Format)
            throw new NotSupportedException($"Invalid IFF header, expected \"FORM\", found \"{formatChunk.TypeId}\"");

        var formatId = s.FixedLengthString("FormatId", IffChunkType.PackedBitmap, 4);
        if (formatId != IffChunkType.PackedBitmap)
            throw new NotSupportedException($"Invalid IFF header, expected \"PBM \", found \"{formatId}\"");

        if (s.IsReading())
        {
            int i = 0;
            while (s.BytesRemaining > 0)
            {
                var chunk = IffChunk.Serdes(i, null, s);
                switch (chunk.TypeId)
                {
                    case IffChunkType.BitmapHeader: img.SerdesHeader(s, chunk.Length); break;
                    case IffChunkType.ColorMapping: img.SerdesPalette(s, chunk.Length); break;
                    case IffChunkType.Hotspot: img.SerdesHotspot(s, chunk.Length); break;

                    case IffChunkType.ColorRanges:
                        img.ColorRanges ??= [];
                        img.ColorRanges.Add(ColorRange.Serdes(img.ColorRanges.Count, null, s));
                        break;

                    case IffChunkType.Thumbnail: img.SerdesThumbnail(s, chunk.Length); break;
                    case IffChunkType.Body: img.SerdesPixels(s, chunk.Length); break;
                    default:
                        s.Bytes("Unk", null, chunk.Length);
                        break;
                }

                i++;
            }
        }
        else
        {
            WriteChunk(s, IffChunkType.BitmapHeader, (x, n) => img.SerdesHeader(x, n));
            WriteChunk(s, IffChunkType.ColorMapping, (x, n) => img.SerdesPalette(x, n));
            WriteChunk(s, IffChunkType.Hotspot,      (x, n) => img.SerdesHotspot(x, n));
            s.List(nameof(img.ColorRanges), img.ColorRanges, img.ColorRanges.Count, ColorRange.Serdes);
            WriteChunk(s, IffChunkType.Thumbnail,    (x, n) => img.SerdesThumbnail(x, n));
            WriteChunk(s, IffChunkType.Body,         (x, n) => img.SerdesPixels(x, n));
        }

        formatChunk.WriteLength(s);
        return img;
    }

    static void WriteChunk(ISerializer s, string chunkType, Action<ISerializer, int> serdes)
    {
        var chunk = IffChunk.Serdes(0, new IffChunk(chunkType, 0), s);
        serdes(s, chunk.Length);
        chunk.WriteLength(s);
    }

    void SerdesHeader(ISerializer s, int length)
    {
        if (length < 20)
            throw new FormatException($"ILBM header chunk was {length} bytes, expected at least 20");

        Width       = s.Int16BE(nameof(Width      ), Width      ); // 0
        Height      = s.Int16BE(nameof(Height     ), Height     ); // 2
        PosX        = s.Int16BE(nameof(PosX       ), PosX       ); // 4
        PosY        = s.Int16BE(nameof(PosY       ), PosY       ); // 6
        NumPlanes   =   s.UInt8(nameof(NumPlanes  ), NumPlanes  ); // 8
        Mask        =   s.UInt8(nameof(Mask       ), Mask       ); // 9
        Compression =   s.UInt8(nameof(Compression), Compression); // A
        Padding     =   s.UInt8(nameof(Padding    ), Padding    ); // B
        Transparent = s.Int16BE(nameof(Transparent), Transparent); // C
        AspectRatio = s.Int16BE(nameof(AspectRatio), AspectRatio); // 8
        PageWidth   = s.Int16BE(nameof(PageWidth  ), PageWidth  ); // 10
        PageHeight  = s.Int16BE(nameof(PageHeight ), PageHeight ); // 12

        if (length > 20)
            s.Bytes("UnexpectedHeaderData", null, length - 20);
    }

    void SerdesPalette(ISerializer s, int length)
    {
        uint[] pal = Palette ?? new uint[length / 3];
        for (int i = 0; i < pal.Length; i++)
        {
            var existing = pal[i];
            pal[i]  =       s.UInt8("R", (byte)( existing &     0xff));
            pal[i] |= (uint)s.UInt8("G", (byte)((existing &   0xff00) >>  8)) <<  8;
            pal[i] |= (uint)s.UInt8("B", (byte)((existing & 0xff0000) >> 16)) << 16;
            pal[i] |= (uint)0xff << 24; // Alpha
        }

        Palette = pal;
    }

    void SerdesHotspot(ISerializer s, int _)
    {
        HotspotX = s.Int16(nameof(HotspotX), HotspotX);
        HotspotY = s.Int16(nameof(HotspotY), HotspotY);
    }

    void SerdesThumbnail(ISerializer s, int length)
    {
        ThumbnailWidth = s.UInt16BE(nameof(ThumbnailWidth), ThumbnailWidth);
        ThumbnailHeight = s.UInt16BE(nameof(ThumbnailHeight), ThumbnailHeight);

        Thumbnail = Compression == 1
            ? s.IsReading() ? Unpack(s, length - 4) : Pack(Thumbnail, s)
            : s.Bytes("Pixels", null, length - 4);
    }

    void SerdesPixels(ISerializer s, int length)
    {
        ImageData = Compression == 1
            ? s.IsReading() ? Unpack(s, length) : Pack(ImageData, s)
            : s.Bytes("Pixels", ImageData, length);
    }

    // ReSharper disable UnusedParameter.Local
    static byte[] Pack(byte[] data, ISerializer s) => throw new NotImplementedException();
    // ReSharper restore UnusedParameter.Local

    static byte[] Unpack(ISerializer s, int size)
    {
        using MemoryStream ms = new MemoryStream();
        var finalOffset = s.Offset + size;
        while (s.Offset < finalOffset)
        {
            byte n = s.UInt8("Type", 0);

            if (n <= 127)
            {
                var rawBytes = s.Bytes("Raw", null, n + 1);
                ms.Write(rawBytes, 0, rawBytes.Length);
            }
            else if (n >= 129)
            {
                byte value = s.UInt8("Value", 0);
                for (int i = 0; i < 257 - n; i++)
                    ms.WriteByte(value);
            }
        }

        if ((size & 1) != 0)
            s.UInt8("Padding", 0);

        return ms.ToArray();
    }
}