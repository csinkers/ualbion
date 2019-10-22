using System.Collections.Generic;

namespace UAlbion.Formats.Assets
{
    public class BlockList
    {
        public class BlockListEntry
        {
            public byte b1;
            public byte b2;
            public byte b3;
        }

        public byte Width { get; set; }
        public byte Height { get; set; }
        public IList<BlockListEntry> Entries { get; } = new List<BlockListEntry>();
    }
}