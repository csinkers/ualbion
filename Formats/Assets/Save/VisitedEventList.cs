using SerdesNet;
using UAlbion.Api;

namespace UAlbion.Formats.Assets.Save
{
    public class VisitedEventList
    {
        public uint Size { get; set; }
        public ushort NumChunks { get; set; }
        public VisitedEvent[] Contents { get; set; }

        public static VisitedEventList Serdes(int _, VisitedEventList c, ISerializer s)
        {
            c ??= new VisitedEventList();
            c.Size = s.UInt32(nameof(Size), c.Size);
            c.NumChunks = s.UInt16(nameof(NumChunks), c.NumChunks);
            ApiUtil.Assert(c.NumChunks == c.Size / 6);
            c.Contents ??= new VisitedEvent[(c.Size - 2) / 6];
            s.List(nameof(c.Contents), c.Contents, c.Contents.Length, VisitedEvent.Serdes);
            return c;
        }
    }
}
