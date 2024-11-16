using System;
using SerdesNet;
using UAlbion.Config;
using UAlbion.Formats.Assets.Inv;

namespace UAlbion.Formats.Parsers;

public class MerchantLoader : IAssetLoader<Inventory>
{
    public Inventory Serdes(Inventory existing, ISerdes s, AssetLoadContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        return Inventory.SerdesMerchant(context.AssetId.ToInt32(), existing, context.Mapping, s);
    }

    public object Serdes(object existing, ISerdes s, AssetLoadContext context)
        => Serdes(existing as Inventory, s, context);
}
