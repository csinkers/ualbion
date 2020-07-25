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
            s.Begin();
            c.Size = s.UInt32(nameof(Size), c.Size);
            c.NumChunks = s.UInt16(nameof(NumChunks), c.NumChunks);
            ApiUtil.Assert(c.NumChunks == c.Size / VisitedEvent.SizeOnDisk);
            c.Contents ??= new VisitedEvent[(c.Size - 2) / VisitedEvent.SizeOnDisk];
            s.List(nameof(c.Contents), c.Contents, c.Contents.Length, VisitedEvent.Serdes);
            s.End();
            return c;
        }
    }
}
