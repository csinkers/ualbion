using System.IO;

namespace UAlbion.Formats.Assets.Flic
{
    public class UnknownChunk : FlicChunk
    {
        public UnknownChunk(FlicChunkType type)
        {
            Type = type;
        }

        public override FlicChunkType Type { get; }
        protected override uint LoadChunk(uint length, BinaryReader br)
        {
            Bytes = br.ReadBytes((int)length);
            return (uint)Bytes.Length;
        }

        public byte[] Bytes { get; private set; }
    }
}