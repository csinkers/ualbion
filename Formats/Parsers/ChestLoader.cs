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
                chest.Slots[i] = s.Meta($"Slot{i}", chest.Slots[i], ItemSlotLoader.Serdes);

            if (s.Offset - start >= length) // If it's a merchant record then we're all done
                return;

            chest.Gold = s.UInt16("Gold", chest.Gold);
            chest.Rations = s.UInt16("Rations", chest.Rations);
        }

        public object Load(BinaryReader br, long streamLength, string name, AssetInfo config)
        {
            var chest = new Chest();
            Translate(chest, new GenericBinaryReader(br, streamLength), streamLength);
            return chest;
        }
    }
}
