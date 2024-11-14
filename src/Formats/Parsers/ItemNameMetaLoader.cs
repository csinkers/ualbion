﻿using System;
using System.Collections.Generic;
using SerdesNet;
using UAlbion.Api.Eventing;
using UAlbion.Config;
using UAlbion.Config.Properties;
using UAlbion.Formats.Assets;

namespace UAlbion.Formats.Parsers;

public class ItemNameMetaLoader : Component, IAssetLoader<ListStringSet>
{
    public static readonly AssetIdAssetProperty TargetProperty = new("Target");
    public ListStringSet Serdes(ListStringSet existing, ISerializer s, AssetLoadContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        var targetId = context.GetProperty(TargetProperty);
        if (targetId == AssetId.None)
            return null;

        if (existing != null)
            throw new NotSupportedException($"{nameof(ItemNameMetaLoader)} is read-only");

        var applier = Resolve<IModApplier>();
        var target = applier.LoadAssetCached(targetId);

        if (target is not Dictionary<string, ListStringSet> dict)
            throw new FormatException($"Expected target \"{targetId}\" to be a {nameof(Dictionary<string, ListStringSet>)} but it was {target?.GetType()}");

        return dict.GetValueOrDefault(context.Language);
    }

    public object Serdes(object existing, ISerializer s, AssetLoadContext context)
        => Serdes((ListStringSet)existing, s, context);
}