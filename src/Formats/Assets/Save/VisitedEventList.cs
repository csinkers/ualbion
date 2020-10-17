using System;
using SerdesNet;
using UAlbion.Api;
using UAlbion.Config;

namespace UAlbion.Formats.Assets.Save
{
    public class VisitedEventList
    {
        public uint Size { get; set; }
        public ushort NumChunks { get; set; }
        public VisitedEvent[] Contents { get; set; }

        public static VisitedEventList Serdes(int _, VisitedEventList c, AssetMapping mapping, ISerializer s)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));
            c ??= new VisitedEventList();
            c.Size = s.UInt32(nameof(Size), c.Size);
            c.NumChunks = s.UInt16(nameof(NumChunks), c.NumChunks);
            ApiUtil.Assert(c.NumChunks == c.Size / VisitedEvent.SizeOnDisk);
            c.Contents ??= new VisitedEvent[(c.Size - 2) / VisitedEvent.SizeOnDisk];
            s.List(nameof(c.Contents), c.Contents, mapping, c.Contents.Length, VisitedEvent.Serdes);
            return c;
        }
    }
}
