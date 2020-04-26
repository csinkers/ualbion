using System.IO;
using SerdesNet;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Config;

namespace UAlbion.Formats.Parsers
{
    [AssetLoader(FileFormat.ChestInventory, FileFormat.MerchantInventory)]
    class ChestLoader : IAssetLoader<Inventory>
    {
        public Inventory Serdes(Inventory chest, ISerializer s, string name, AssetInfo config) =>
            config.Format == FileFormat.ChestInventory
                ? Inventory.SerdesChest(config.Id, chest, s)
                : Inventory.SerdesMerchant(config.Id, chest, s);

        public object Load(BinaryReader br, long streamLength, string name, AssetInfo config)
            => Serdes(null, new AlbionReader(br, streamLength), name, config);
    }
}