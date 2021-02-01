using System;
using System.Collections.Generic;
using SerdesNet;

namespace UAlbion.Formats.Assets.Flic
{
    public class FlicFile
    {
        public FlicFile(ISerializer s)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));
            if (s.IsWriting())
                throw new NotImplementedException("FLIC file writing not currently supported");

            long startOffset = s.Offset;
            Size        =  s.UInt32(null, 0); // Size of FLIC including this header
            Type        =  (FlicHeaderType)s.UInt16(null, 0); // File type 0xAF11, 0xAF12, 0xAF30, 0xAF44, ...
            Frames      =  s.UInt16(null, 0); // Number of frames in first segment
            Width       =  s.UInt16(null, 0); // FLIC width in pixels
            Height      =  s.UInt16(null, 0); // FLIC height in pixels
            Depth       =  s.UInt16(null, 0); // Bits per pixel (usually 8)
            Flags       =  s.UInt16(null, 0); // Set to zero or to three
            Speed       =  s.UInt32(null, 0); // Delay between frames in jiffies (1/70 s)
            Reserved1   =  s.UInt16(null, 0); // Set to zero
            Created     =  s.UInt32(null, 0); // Date of FLIC creation (FLC only)
            Creator     =  s.UInt32(null, 0); // Serial number or compiler id (FLC only)
            Updated     =  s.UInt32(null, 0); // Date of FLIC update (FLC only)
            Updater     =  s.UInt32(null, 0); // Serial number (FLC only), see creator
            AspectDx    =  s.UInt16(null, 0); // Width of square rectangle (FLC only)
            AspectDy    =  s.UInt16(null, 0); // Height of square rectangle (FLC only)
            ExtFlags    =  s.UInt16(null, 0); // EGI: flags for specific EGI extensions
            KeyFrames   =  s.UInt16(null, 0); // EGI: key-image frequency
            TotalFrames =  s.UInt16(null, 0); // EGI: total number of frames (segments)
            ReqMemory   =  s.UInt32(null, 0); // EGI: maximum chunk size (uncompressed)
            MaxRegions  =  s.UInt16(null, 0); // EGI: max. number of regions in a CHK_REGION chunk
            TranspNum   =  s.UInt16(null, 0); // EGI: number of transparent levels
            Reserved2   =  s.ByteArray(null, null, 24); // Set to zero
            OFrame1     =  s.UInt32(null, 0); // Offset to frame 1 (FLC only)
            OFrame2     =  s.UInt32(null, 0); // Offset to frame 2 (FLC only)
            Reserved3   =  s.ByteArray(null, null, 40); // Set to zero

            var finalOffset = startOffset + Size;
            while (s.Offset < finalOffset)
                Chunks.Add(FlicChunk.Load(s, Width, Height));
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
        public FlicPlayer Play(byte[] pixelData) => new FlicPlayer(this, pixelData);
    }
}
