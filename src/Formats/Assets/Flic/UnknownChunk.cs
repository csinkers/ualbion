using SerdesNet;

namespace UAlbion.Formats.Assets.Flic
{
    public class UnknownChunk : FlicChunk
    {
        public UnknownChunk(FlicChunkType type)
        {
            Type = type;
        }

        public override FlicChunkType Type { get; }
        protected override uint SerdesBody(uint length, ISerializer s)
        {
            s.Begin("Unk:" + Type);
            Bytes = s.ByteArray(nameof(Bytes), Bytes, (int)length);
            s.End();
            return (uint)Bytes.Length;
        }

        public byte[] Bytes { get; private set; }
    }
}