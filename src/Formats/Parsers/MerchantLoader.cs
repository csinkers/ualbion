using System;
using SerdesNet;
using UAlbion.Config;
using UAlbion.Formats.Assets;

namespace UAlbion.Formats.Parsers
{
    public class MerchantLoader : IAssetLoader<Inventory>
    {
        public Inventory Serdes(Inventory existing, AssetInfo config, AssetMapping mapping, ISerializer s)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));
            return Inventory.SerdesMerchant(config.Id, existing, mapping, s);
        }

        public object Serdes(object existing, AssetInfo config, AssetMapping mapping, ISerializer s)
            => Serdes(existing as Inventory, config, mapping, s);
    }
}