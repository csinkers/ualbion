using System;
using System.IO;

namespace UAlbion.Formats.Assets.Flic
{
    public class Palette6Chunk : FlicChunk
    {
        public override FlicChunkType Type => FlicChunkType.Palette6Bit;
        protected override uint LoadChunk(uint length, BinaryReader br)
        {
            throw new NotImplementedException();
        }
    }
}