using System;
using System.IO;

namespace UAlbion.Formats.Assets.Flic
{
    public class CopyChunk : FlicChunk
    {
        public byte[] PixelData { get; private set; }
        public override FlicChunkType Type => FlicChunkType.FullUncompressed;
        protected override uint LoadChunk(uint length, BinaryReader br)
        {
            if (br == null) throw new ArgumentNullException(nameof(br));
            PixelData = br.ReadBytes((int)length);
            return length;
        }
    }
}