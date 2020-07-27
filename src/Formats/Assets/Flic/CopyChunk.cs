using SerdesNet;

namespace UAlbion.Formats.Assets.Flic
{
    public class CopyChunk : FlicChunk
    {
        public byte[] PixelData { get; private set; }

        public override FlicChunkType Type => FlicChunkType.FullUncompressed;
        protected override uint SerdesBody(uint length, ISerializer s)
        {
            PixelData = s.ByteArray(nameof(PixelData), PixelData, PixelData?.Length ?? (int)length);
            return length;
        }
    }
}