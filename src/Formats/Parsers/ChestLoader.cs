using System;
using SerdesNet;
using UAlbion.Config;
using UAlbion.Formats.Assets;

namespace UAlbion.Formats.Parsers;

public class ChestLoader : IAssetLoader<Inventory>
{
    public Inventory Serdes(Inventory existing, ISerializer s, AssetLoadContext context)
    {
        if (context == null) throw new ArgumentNullException(nameof(context));
        return Inventory.SerdesChest(context.AssetId.ToInt32(), existing, context.Mapping, s);
    }

    public object Serdes(object existing, ISerializer s, AssetLoadContext context)
        => Serdes(existing as Inventory, s, context);
}
