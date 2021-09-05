using System;
using SerdesNet;
using UAlbion.Api;
using UAlbion.Config;
using UAlbion.Formats.Assets;

namespace UAlbion.Formats.Parsers
{
    public class ChestLoader : IAssetLoader<Inventory>
    {
        public Inventory Serdes(Inventory existing, AssetInfo info, AssetMapping mapping, ISerializer s, IJsonUtil jsonUtil)
        {
            if (info == null) throw new ArgumentNullException(nameof(info));
            return Inventory.SerdesChest(info.AssetId.ToInt32(), existing, mapping, s);
        }

        public object Serdes(object existing, AssetInfo info, AssetMapping mapping, ISerializer s, IJsonUtil jsonUtil)
            => Serdes(existing as Inventory, info, mapping, s, jsonUtil);
    }
}
