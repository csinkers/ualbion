using System.IO;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Config;

namespace UAlbion.Formats.Parsers
{
    [AssetLoader(FileFormat.BlockList)]
    public class BlockListLoader : IAssetLoader
    {
        public object Load(BinaryReader br, long streamLength, string name, AssetInfo config)
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
