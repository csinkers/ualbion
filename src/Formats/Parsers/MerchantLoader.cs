﻿using System;
using SerdesNet;
using UAlbion.Config;
using UAlbion.Formats.Assets;

namespace UAlbion.Formats.Parsers
{
    public class MerchantLoader : IAssetLoader<Inventory>
    {
        public Inventory Serdes(Inventory existing, AssetInfo info, AssetMapping mapping, ISerializer s)
        {
            if (info == null) throw new ArgumentNullException(nameof(info));
            return Inventory.SerdesMerchant(info.AssetId.ToInt32(), existing, mapping, s);
        }

        public object Serdes(object existing, AssetInfo info, AssetMapping mapping, ISerializer s)
            => Serdes(existing as Inventory, info, mapping, s);
    }
}
