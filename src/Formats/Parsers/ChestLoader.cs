using System;
using System.IO;
using SerdesNet;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Config;

namespace UAlbion.Formats.Parsers
{
    [AssetLoader(FileFormat.ChestInventory, FileFormat.MerchantInventory)]
    public class ChestLoader : IAssetLoader<Inventory>
    {
        public Inventory Serdes(Inventory chest, ISerializer s, AssetKey key, AssetInfo config)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));
            return config.Format == FileFormat.ChestInventory
                ? Inventory.SerdesChest(config.Id, chest, s)
                : Inventory.SerdesMerchant(config.Id, chest, s);
        }

        public object Load(BinaryReader br, long streamLength, AssetKey key, AssetInfo config)
            => Serdes(null, new AlbionReader(br, streamLength), key, config);
    }
}