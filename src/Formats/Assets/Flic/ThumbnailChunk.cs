using System;
using SerdesNet;

namespace UAlbion.Formats.Assets.Flic
{
    public class ThumbnailChunk : FlicChunk
    {
        public override FlicChunkType Type => FlicChunkType.Thumbnail;
        protected override uint SerdesBody(uint length, ISerializer s)
        {
            throw new NotImplementedException();
        }
    }
}