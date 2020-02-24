using System.Diagnostics;
using UAlbion.Formats.Parsers;

namespace UAlbion.Formats.Assets.Save
{
    public class MysteryChunk6
    {
        public uint Size { get; set; }
        public ushort NumChunks { get; set; }
        public UnkSixByte[] Contents { get; set; }

        public static MysteryChunk6 Serdes(int _, MysteryChunk6 c, ISerializer s)
        {
            c ??= new MysteryChunk6();
            c.Size = s.UInt32(nameof(Size), c.Size);
            c.NumChunks = s.UInt16(nameof(NumChunks), c.NumChunks);
            Debug.Assert(c.NumChunks == c.Size / 6);
            c.Contents ??= new UnkSixByte[(c.Size - 2) / 6];
            for (int i = 0; i < c.Contents.Length; i++)
                c.Contents[i] = UnkSixByte.Serdes(c.Contents[i], s);
            return c;
        }
    }
}