using System.IO;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Config;

namespace UAlbion.Formats.Parsers
{
    [AssetLoader(FileFormat.Inventory)]
    class ChestLoader : IAssetLoader
    {
        static void Translate(Chest chest, ISerializer s, long length)
        {
            var start = s.Offset;
            for (int i = 0; i < Chest.SlotCount; i++)
                s.Meta($"Slot{i}", ItemSlotLoader.Write(chest.Slots[i]), ItemSlotLoader.Read(x => chest.Slots[i] = x));

            if (s.Offset - start >= length) // If it's a merchant record then we're all done
                return;

            s.UInt16("Gold", () => chest.Gold, x => chest.Gold = x);
            s.UInt16("Rations", () => chest.Rations, x => chest.Rations = x);
        }

        public object Load(BinaryReader br, long streamLength, string name, AssetInfo config)
        {
            var chest = new Chest();
            Translate(chest, new GenericBinaryReader(br), streamLength);
            return chest;
        }
    }
}
