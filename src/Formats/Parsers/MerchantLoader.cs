using System;
using SerdesNet;
using UAlbion.Config;
using UAlbion.Formats.Assets;

namespace UAlbion.Formats.Parsers;

public class MerchantLoader : IAssetLoader<Inventory>
{
    public Inventory Serdes(Inventory existing, AssetInfo info, ISerializer s, LoaderContext context)
    {
        if (info == null) throw new ArgumentNullException(nameof(info));
        return Inventory.SerdesMerchant(info.AssetId.ToInt32(), existing, context.Mapping, s);
    }

    public object Serdes(object existing, AssetInfo info, ISerializer s, LoaderContext context)
        => Serdes(existing as Inventory, info, s, context);
}
