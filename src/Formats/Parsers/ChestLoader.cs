using System;
using System.IO;
using SerdesNet;
using UAlbion.Config;
using UAlbion.Formats.Assets;

namespace UAlbion.Formats.Parsers
{
    [AssetLoader(FileFormat.ChestInventory, FileFormat.MerchantInventory)]
    public class ChestLoader : IAssetLoader<Inventory>
    {
        public Inventory Serdes(Inventory chest, AssetMapping mapping, ISerializer s, AssetId id, AssetInfo config)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));
            return config.Format == FileFormat.ChestInventory
                ? Inventory.SerdesChest(config.Id, chest, mapping, s)
                : Inventory.SerdesMerchant(config.Id, chest, mapping, s);
        }

        public object Load(BinaryReader br, long streamLength, AssetMapping mapping, AssetId id, AssetInfo config)
            => Serdes(null, mapping, new AlbionReader(br, streamLength), id, config);
    }
}
