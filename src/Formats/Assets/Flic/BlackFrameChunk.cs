using System;
using System.IO;

namespace UAlbion.Formats.Assets.Flic
{
    public class BlackFrameChunk : FlicChunk
    {
        public override FlicChunkType Type => FlicChunkType.BlackFrameData;
        protected override uint LoadChunk(uint length, BinaryReader br)
        {
            throw new NotImplementedException();
        }
    }
}