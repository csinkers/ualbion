﻿using System;
using SerdesNet;
using UAlbion.Api.Eventing;
using UAlbion.Config;
using UAlbion.Formats.Assets.Inv;

namespace UAlbion.Formats.Parsers;

public class ItemDataLoader : Component, IAssetLoader<ItemData>
{
    public ItemData Serdes(ItemData existing, ISerdes s, AssetLoadContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        return ItemData.Serdes(context.AssetId, existing, s, Resolve<ISpellManager>());
    }

    public object Serdes(object existing, ISerdes s, AssetLoadContext context)
        => Serdes(existing as ItemData, s, context);
}