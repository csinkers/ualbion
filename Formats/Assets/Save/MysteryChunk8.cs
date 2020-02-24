using System.Diagnostics;
using UAlbion.Formats.Parsers;

namespace UAlbion.Formats.Assets.Save
{
    public class MysteryChunk8
    {
        public enum ChunkType
        {
            MysterySmall = 0x3,
            Xld = 0x11,
            Mystery6Byte = 0xc8,
        }

        public uint Size { get; set; }
        public ushort NumChunks { get; set; }
        public UnkEightByte[] Contents { get; set; }

        public static MysteryChunk8 Serdes(int _, MysteryChunk8 c, ISerializer s)
        {
            c ??= new MysteryChunk8();
            c.Size = s.UInt32(nameof(Size), c.Size);
            c.NumChunks = s.UInt16(nameof(NumChunks), c.NumChunks);
            Debug.Assert(c.NumChunks == c.Size / 8);
            c.Contents ??= new UnkEightByte[(c.Size - 2) / 8];
            for (int i = 0; i < c.Contents.Length; i++)
                c.Contents[i] = UnkEightByte.Serdes(c.Contents[i], s);

            return c;
        }
    }
}