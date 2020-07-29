using System.Collections.Generic;
using System.IO;
using System.Linq;
using UAlbion.Api;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Config;

namespace UAlbion.Formats.Assets.Flic
{
    public class FlicLoader : IAssetLoader
    {
        public object Load(BinaryReader br, long streamLength, AssetKey key, AssetInfo config) 
            => new FlicFile(br);
    }
    public class FlicFile
    {
        public FlicFile(BinaryReader br)
        {
            long startOffset = br.BaseStream.Position;
            Size        =  br.ReadUInt32(); // Size of FLIC including this header
            Type        =  (FlicHeaderType)br.ReadUInt16(); // File type 0xAF11, 0xAF12, 0xAF30, 0xAF44, ...
            Frames      =  br.ReadUInt16(); // Number of frames in first segment
            Width       =  br.ReadUInt16(); // FLIC width in pixels
            Height      =  br.ReadUInt16(); // FLIC height in pixels
            Depth       =  br.ReadUInt16(); // Bits per pixel (usually 8)
            Flags       =  br.ReadUInt16(); // Set to zero or to three
            Speed       =  br.ReadUInt32(); // Delay between frames in jiffied (1/70 s)
            Reserved1   =  br.ReadUInt16(); // Set to zero
            Created     =  br.ReadUInt32(); // Date of FLIC creation (FLC only)
            Creator     =  br.ReadUInt32(); // Serial number or compiler id (FLC only)
            Updated     =  br.ReadUInt32(); // Date of FLIC update (FLC only)
            Updater     =  br.ReadUInt32(); // Serial number (FLC only), see creator
            AspectDx    =  br.ReadUInt16(); // Width of square rectangle (FLC only)
            AspectDy    =  br.ReadUInt16(); // Height of square rectangle (FLC only)
            ExtFlags    =  br.ReadUInt16(); // EGI: flags for specific EGI extensions
            KeyFrames   =  br.ReadUInt16(); // EGI: key-image frequency
            TotalFrames =  br.ReadUInt16(); // EGI: total number of frames (segments)
            ReqMemory   =  br.ReadUInt32(); // EGI: maximum chunk size (uncompressed)
            MaxRegions  =  br.ReadUInt16(); // EGI: max. number of regions in a CHK_REGION chunk
            TranspNum   =  br.ReadUInt16(); // EGI: number of transparent levels
            Reserved2   =  br.ReadBytes(24); // Set to zero
            OFrame1     =  br.ReadUInt32(); // Offset to frame 1 (FLC only)
            OFrame2     =  br.ReadUInt32(); // Offset to frame 2 (FLC only)
            Reserved3   =  br.ReadBytes(40); // Set to zero

            var finalOffset = startOffset + Size;
            while (br.BaseStream.Position < finalOffset)
                Chunks.Add(FlicChunk.Load(br, Width, Height));
        }

        public uint Size { get; } // Size of FLIC including this header
        public FlicHeaderType Type { get; } // File type 0xAF11, 0xAF12, 0xAF30, 0xAF44, ...
        public ushort Frames { get; } // Number of frames in first segment
        public ushort Width { get; } // FLIC width in pixels
        public ushort Height { get; } // FLIC height in pixels
        public ushort Depth { get; } // Bits per pixel (usually 8)
        public ushort Flags { get; } // Set to zero or to three
        public uint Speed { get; } // Delay between frames in jiffies (1/70th of a second)
        public ushort Reserved1 { get; } // Set to zero
        public uint Created { get; } // Date of FLIC creation (FLC only)
        public uint Creator { get; } // Serial number or compiler id (FLC only)
        public uint Updated { get; } // Date of FLIC update (FLC only)
        public uint Updater { get; } // Serial number (FLC only), see creator
        public ushort AspectDx { get; } // Width of square rectangle (FLC only)
        public ushort AspectDy { get; } // Height of square rectangle (FLC only)
        public ushort ExtFlags { get; } // EGI: flags for specific EGI extensions
        public ushort KeyFrames { get; } // EGI: key-image frequency
        public ushort TotalFrames { get; } // EGI: total number of frames (segments)
        public uint ReqMemory { get; } // EGI: maximum chunk size (uncompressed)
        public ushort MaxRegions { get; } // EGI: max. number of regions in a CHK_REGION chunk
        public ushort TranspNum { get; } // EGI: number of transparent levels
        public byte[] Reserved2 { get; } // Set to zero (24 bytes)
        public uint OFrame1 { get; } // Offset to frame 1 (FLC only)
        public uint OFrame2 { get; } // Offset to frame 2 (FLC only)
        public byte[] Reserved3 { get; } // Set to zero (40 bytes)
        public IList<FlicChunk> Chunks { get; } = new List<FlicChunk>();

        public IEnumerable<uint[]> AllFrames()
        {
            uint[] palette = new uint[0x100];
            byte[] buffer8 = new byte[Width * Height];
            uint[] buffer24 = new uint[Width * Height];
            foreach (var frame in Chunks.OfType<FlicFrame>())
            {
                ApiUtil.Assert(frame.Width == 0, "Frame width overrides are not currently handled");
                ApiUtil.Assert(frame.Height == 0, "Frame height overrides are not currently handled");

                foreach (var subChunk in frame.SubChunks)
                {
                    if (subChunk is Palette8Chunk paletteChunk)
                        palette = paletteChunk.GetEffectivePalette(palette);
                    else if (subChunk is CopyChunk copy)
                        copy.PixelData.CopyTo(buffer8, 0);
                    else if (subChunk is DeltaFlcChunk delta)
                        delta.Apply(buffer8, Width);
                }
                // Inefficient, could be optimised by rendering with an 8-bit shader or
                // getting the deltas to apply directly to the 24-bit buffer.
                for (int y = 0; y < Height; y++)
                    for (int x = 0; x < Width; x++)
                        buffer24[(Height-y-1) * Width + x] = palette[buffer8[y * Width + x]];

                yield return buffer24;
            }
        }
    }
}
