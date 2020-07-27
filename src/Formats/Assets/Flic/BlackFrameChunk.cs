using System;
using SerdesNet;

namespace UAlbion.Formats.Assets.Flic
{
    public class BlackFrameChunk : FlicChunk
    {
        public override FlicChunkType Type => FlicChunkType.BlackFrameData;
        protected override uint SerdesBody(uint length, ISerializer s)
        {
            throw new NotImplementedException();
        }
    }
}