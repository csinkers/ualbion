using System;
using System.Collections.Generic;
using System.IO;
using SerdesNet;

namespace UAlbion.Formats.Assets
{
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
        public IList<ColorRange> ColorRanges { get; set; }
        public ushort ThumbnailWidth { get; set; }
        public ushort ThumbnailHeight { get; set; }
        public byte[] Thumbnail { get; set; }
        public byte[] ImageData { get; set; }

        public static InterlacedBitmap Serdes(InterlacedBitmap img, ISerializer s)
        {
            img ??= new InterlacedBitmap();
            s.Begin();

            var formatChunk = IFFChunk.Serdes(0, new IFFChunk(IFFChunkType.Format, 0), s);
            if (formatChunk.TypeId != IFFChunkType.Format)
                throw new NotSupportedException($"Invalid IFF header, expected \"FORM\", found \"{formatChunk.TypeId}\"");

            var formatId = s.FixedLengthString("FormatId", IFFChunkType.PackedBitmap, 4);
            if (formatId != IFFChunkType.PackedBitmap)
                throw new NotSupportedException($"Invalid IFF header, expected \"PBM \", found \"{formatId}\"");

            if (s.Mode == SerializerMode.Reading)
            {
                int i = 0;
                while(s.BytesRemaining > 0)
                {
                    var chunk = IFFChunk.Serdes(i, null, s);
                    switch (chunk.TypeId)
                    {
                        case IFFChunkType.BitmapHeader: img.SerdesHeader(s, chunk.Length); break;
                        case IFFChunkType.ColorMapping: img.SerdesPalette(s, chunk.Length); break;
                        case IFFChunkType.Hotspot: img.SerdesHotspot(s, chunk.Length); break;

                        case IFFChunkType.ColorRanges:
                            img.ColorRanges ??= new List<ColorRange>();
                            img.ColorRanges.Add(ColorRange.Serdes(img.ColorRanges.Count, null, s)); 
                            break;

                        case IFFChunkType.Thumbnail: img.SerdesThumbnail(s, chunk.Length); break;
                        case IFFChunkType.Body: img.SerdesPixels(s, chunk.Length); break;
                        default:
                            s.ByteArray("Unk", null, chunk.Length);
                            break;
                    }

                    s.Check();
                    i++;
                }
            }
            else
            {
                img.WriteChunk(s, IFFChunkType.BitmapHeader, (x, n) => img.SerdesHeader(x, n));
                img.WriteChunk(s, IFFChunkType.ColorMapping, (x, n) => img.SerdesPalette(x, n));
                img.WriteChunk(s, IFFChunkType.Hotspot,      (x, n) => img.SerdesHotspot(x, n));
                s.List(nameof(img.ColorRanges), img.ColorRanges, img.ColorRanges.Count, ColorRange.Serdes);
                img.WriteChunk(s, IFFChunkType.Thumbnail,    (x, n) => img.SerdesThumbnail(x, n));
                img.WriteChunk(s, IFFChunkType.Body,         (x, n) => img.SerdesPixels(x, n));
            }

            formatChunk.WriteLength(s);
            s.End();
            return img;
        }

        void WriteChunk(ISerializer s, string chunkType, Action<ISerializer, int> serdes)
        {
            var chunk = IFFChunk.Serdes(0, new IFFChunk(chunkType, 0), s);
            serdes(s, chunk.Length);
            chunk.WriteLength(s);
        }

        void SerdesHeader(ISerializer s, int _)
        {
            Width       = s.Int16BE(nameof(Width      ), Width      );
            Height      = s.Int16BE(nameof(Height     ), Height     );
            PosX        = s.Int16BE(nameof(PosX       ), PosX       );
            PosY        = s.Int16BE(nameof(PosY       ), PosY       );
            NumPlanes   =   s.UInt8(nameof(NumPlanes  ), NumPlanes  );
            Mask        =   s.UInt8(nameof(Mask       ), Mask       );
            Compression =   s.UInt8(nameof(Compression), Compression);
            Padding     =   s.UInt8(nameof(Padding    ), Padding    );
            Transparent = s.Int16BE(nameof(Transparent), Transparent);
            AspectRatio = s.Int16BE(nameof(AspectRatio), AspectRatio);
            PageWidth   = s.Int16BE(nameof(PageWidth  ), PageWidth  );
            PageHeight  = s.Int16BE(nameof(PageHeight ), PageHeight );
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
                ? s.Mode == SerializerMode.Reading ? Unpack(s, length - 4) : Pack(Thumbnail, s)
                : s.ByteArray("Pixels", null, length - 4);
        }

        void SerdesPixels(ISerializer s, int length)
        {
            ImageData = Compression == 1
                ? s.Mode == SerializerMode.Reading ? Unpack(s, length) : Pack(ImageData, s)
                : s.ByteArray("Pixels", ImageData, length);
        }

        static byte[] Pack(byte[] data, ISerializer s)
        {
            throw new NotImplementedException();
        }

        static byte[] Unpack(ISerializer s, int size)
        {
            using MemoryStream ms = new MemoryStream();
            var finalOffset = s.Offset + size;
            while (s.Offset < finalOffset)
            {
                byte n = s.UInt8("Type", 0);

                if (n <= 127)
                {
                    var rawBytes = s.ByteArray("Raw", null, n + 1);
                    ms.Write(rawBytes, 0, rawBytes.Length);
                }
                else if (129 <= n && n <= 255)
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
}
