using System;
using SerdesNet;
using UAlbion.Config;
using UAlbion.Formats.Assets;

namespace UAlbion.Formats.Parsers;

public class ChestLoader : IAssetLoader<Inventory>
{
    public Inventory Serdes(Inventory existing, AssetInfo info, ISerializer s, SerdesContext context)
    {
        if (info == null) throw new ArgumentNullException(nameof(info));
        return Inventory.SerdesChest(info.AssetId.ToInt32(), existing, context.Mapping, s);
    }

    public object Serdes(object existing, AssetInfo info, ISerializer s, SerdesContext context)
        => Serdes(existing as Inventory, info, s, context);
}
