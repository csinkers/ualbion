using System.Collections.Generic;
using System.IO;

namespace UAlbion.Formats.Parsers
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

    [AssetLoader(XldObjectType.BlockList)]
    public class BlockListLoader : IAssetLoader
    {
        public object Load(BinaryReader br, long streamLength, string name, AssetConfig.Asset config)
        {
            var bl = new BlockList();
            bl.Width = br.ReadByte();
            bl.Height = br.ReadByte();
            if (bl.Width == 0 && bl.Height == 0) return null;

            for(int j = 0; j < bl.Height; j++)
            {
                for(int i = 0; i < bl.Width; i++)
                {
                    var ble = new BlockList.BlockListEntry();
                    ble.b1 = br.ReadByte();
                    ble.b2 = br.ReadByte();
                    ble.b3 = br.ReadByte();
                    bl.Entries.Add(ble);
                }
            }

            return bl;
        }
    }
}