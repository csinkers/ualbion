using System.Collections.Generic;
using System.Linq;
using SerdesNet;
using UAlbion.Api;

namespace UAlbion.Formats.Assets.Flic
{
    public class FlicFile
    {
        public static FlicFile Serdes(FlicFile f, ISerializer s)
        {
            f ??= new FlicFile();
            var initialOffset = s.Offset;
            f.Size        = s.UInt32(nameof(Size),         f.Size         ); // Size of FLIC including this header
            f.Type        = s.EnumU16(nameof(Type),        f.Type         ); // File type 0xAF11, 0xAF12, 0xAF30, 0xAF44, ...
            f.Frames      = s.UInt16(nameof(Frames),       f.Frames       ); // Number of frames in first segment
            f.Width       = s.UInt16(nameof(Width),        f.Width        ); // FLIC width in pixels
            f.Height      = s.UInt16(nameof(Height),       f.Height       ); // FLIC height in pixels
            f.Depth       = s.UInt16(nameof(Depth),        f.Depth        ); // Bits per pixel (usually 8)
            f.Flags       = s.UInt16(nameof(Flags),        f.Flags        ); // Set to zero or to three
            f.Speed       = s.UInt32(nameof(Speed),        f.Speed        ); // Delay between frames
            f.Reserved1   = s.UInt16(nameof(Reserved1),    f.Reserved1    ); // Set to zero
            f.Created     = s.UInt32(nameof(Created),      f.Created      ); // Date of FLIC creation (FLC only)
            f.Creator     = s.UInt32(nameof(Creator),      f.Creator      ); // Serial number or compiler id (FLC only)
            f.Updated     = s.UInt32(nameof(Updated),      f.Updated      ); // Date of FLIC update (FLC only)
            f.Updater     = s.UInt32(nameof(Updater),      f.Updater      ); // Serial number (FLC only), see creator
            f.AspectDx    = s.UInt16(nameof(AspectDx),     f.AspectDx     ); // Width of square rectangle (FLC only)
            f.AspectDy    = s.UInt16(nameof(AspectDy),     f.AspectDy     ); // Height of square rectangle (FLC only)
            f.ExtFlags    = s.UInt16(nameof(ExtFlags),     f.ExtFlags     ); // EGI: flags for specific EGI extensions
            f.KeyFrames   = s.UInt16(nameof(KeyFrames),    f.KeyFrames    ); // EGI: key-image frequency
            f.TotalFrames = s.UInt16(nameof(TotalFrames),  f.TotalFrames  ); // EGI: total number of frames (segments)
            f.ReqMemory   = s.UInt32(nameof(ReqMemory),    f.ReqMemory    ); // EGI: maximum chunk size (uncompressed)
            f.MaxRegions  = s.UInt16(nameof(MaxRegions),   f.MaxRegions   ); // EGI: max. number of regions in a CHK_REGION chunk
            f.TranspNum   = s.UInt16(nameof(TranspNum),    f.TranspNum    ); // EGI: number of transparent levels
            f.Reserved2   = s.ByteArray(nameof(Reserved2), f.Reserved2, 24); // Set to zero
            f.OFrame1     = s.UInt32(nameof(OFrame1),      f.OFrame1      ); // Offset to frame 1 (FLC only)
            f.OFrame2     = s.UInt32(nameof(OFrame2),      f.OFrame2      ); // Offset to frame 2 (FLC only)
            f.Reserved3   = s.ByteArray(nameof(Reserved3), f.Reserved3, 40); // Set to zero

            if (f.Chunks.Count > 0)
            {
                s.List(nameof(f.Chunks), f.Chunks, f.Chunks.Count,
                    (i, c, s2) => FlicChunk.Serdes(i, c, s2, f.Width, f.Height));

                var finalOffset = s.Offset;
                s.Seek(initialOffset);
                s.UInt32(nameof(Size), (uint)(s.Offset - initialOffset));
                s.Seek(finalOffset);
            }
            else
            {
                var finalOffset = initialOffset + f.Size;
                int i = 0;
                while (s.Offset < finalOffset)
                    f.Chunks.Add(FlicChunk.Serdes(i++, null, s, f.Width, f.Height));
            }

            return f;
        }

        public uint Size { get; private set; } // Size of FLIC including this header
        public FlicHeaderType Type { get; private set; } // File type 0xAF11, 0xAF12, 0xAF30, 0xAF44, ...
        public ushort Frames { get; private set; } // Number of frames in first segment
        public ushort Width { get; private set; } // FLIC width in pixels
        public ushort Height { get; private set; } // FLIC height in pixels
        public ushort Depth { get; private set; } // Bits per pixel (usually 8)
        public ushort Flags { get; private set; } // Set to zero or to three
        public uint Speed { get; private set; } // Delay between frames in jiffies (1/70th of a second)
        public ushort Reserved1 { get; private set; } // Set to zero
        public uint Created { get; private set; } // Date of FLIC creation (FLC only)
        public uint Creator { get; private set; } // Serial number or compiler id (FLC only)
        public uint Updated { get; private set; } // Date of FLIC update (FLC only)
        public uint Updater { get; private set; } // Serial number (FLC only), see creator
        public ushort AspectDx { get; private set; } // Width of square rectangle (FLC only)
        public ushort AspectDy { get; private set; } // Height of square rectangle (FLC only)
        public ushort ExtFlags { get; private set; } // EGI: flags for specific EGI extensions
        public ushort KeyFrames { get; private set; } // EGI: key-image frequency
        public ushort TotalFrames { get; private set; } // EGI: total number of frames (segments)
        public uint ReqMemory { get; private set; } // EGI: maximum chunk size (uncompressed)
        public ushort MaxRegions { get; private set; } // EGI: max. number of regions in a CHK_REGION chunk
        public ushort TranspNum { get; private set; } // EGI: number of transparent levels
        public byte[] Reserved2 { get; private set; } // Set to zero (24 bytes)
        public uint OFrame1 { get; private set; } // Offset to frame 1 (FLC only)
        public uint OFrame2 { get; private set; } // Offset to frame 2 (FLC only)
        public byte[] Reserved3 { get; private set; } // Set to zero (40 bytes)
        public IList<FlicChunk> Chunks { get; } = new List<FlicChunk>();

        public IEnumerable<byte[]> AllFrames()
        {
            byte[] palette = new byte[256 * 3]; // 0x300
            byte[] buffer8 = new byte[Width * Height];
            byte[] buffer24 = new byte[3 * Width * Height];
            foreach (var frame in Chunks.OfType<FlicFrame>())
            {
                ApiUtil.Assert(frame.Width == 0, "Frame width overrides are not currently handled");
                ApiUtil.Assert(frame.Height == 0, "Frame height overrides are not currently handled");

                foreach (var subChunk in frame.SubChunks)
                {
                    if (subChunk is Palette8Chunk paletteChunk)
                        palette = paletteChunk.GetEffectivePalette(palette);
                }

                yield return buffer24;
            }
        }

        static IEnumerable<byte[]> ApplyPalette(IEnumerable<byte[]> eightBitFrames, byte[] palette)
        {
            byte[] dest = null;
            foreach (var source in eightBitFrames)
            {
                if(dest == null)
                    dest = new byte[source.Length * 3];

                for (int i = 0; i < source.Length; i++)
                {
                    dest[i * 3] = palette[source[i] * 3];
                    dest[i * 3+1] = palette[source[i] * 3 + 1];
                    dest[i * 3+2] = palette[source[i] * 3 + 2];
                }

                yield return dest;
            }
        }
    }
}
