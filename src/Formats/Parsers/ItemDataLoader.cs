using System;
using SerdesNet;
using UAlbion.Api.Eventing;
using UAlbion.Config;
using UAlbion.Formats.Assets;

namespace UAlbion.Formats.Parsers;

public class ItemDataLoader : Component, IAssetLoader<ItemData>
{
    public ItemData Serdes(ItemData existing, ISerializer s, AssetLoadContext context)
    {
        if (context == null) throw new ArgumentNullException(nameof(context));
        return ItemData.Serdes(context.AssetId, existing, s, Resolve<ISpellManager>());
    }

    public object Serdes(object existing, ISerializer s, AssetLoadContext context)
        => Serdes(existing as ItemData, s, context);
}