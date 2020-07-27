using System;
using SerdesNet;

namespace UAlbion.Formats.Assets.Flic
{
    public class Palette6Chunk : FlicChunk
    {
        public override FlicChunkType Type => FlicChunkType.Palette6Bit;
        protected override uint SerdesBody(uint length, ISerializer s)
        {
            throw new NotImplementedException();
        }
    }
}