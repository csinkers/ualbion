using System;
using System.IO;

namespace UAlbion.Formats.Assets.Flic
{
    public class ThumbnailChunk : FlicChunk
    {
        public override FlicChunkType Type => FlicChunkType.Thumbnail;
        protected override uint LoadChunk(uint length, BinaryReader br)
        {
            throw new NotImplementedException();
        }
    }
}