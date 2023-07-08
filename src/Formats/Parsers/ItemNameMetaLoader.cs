using System;
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
        if (context == null) throw new ArgumentNullException(nameof(context));
        var targetId = context.GetProperty(TargetProperty);
        if (targetId == AssetId.None)
            return null;

        if (existing != null)
            throw new NotSupportedException($"{nameof(ItemNameMetaLoader)} is read-only");

        var applier = Resolve<IModApplier>();
        var target = applier.LoadAssetCached(targetId);
        if (target is not Dictionary<string, ListStringSet> dict)
            throw new FormatException($"Expected target \"\" to be a {nameof(Dictionary<string, ListStringSet>)}");

        return dict.TryGetValue(context.Language, out var list)
            ? list
            : null;
    }

    public object Serdes(object existing, ISerializer s, AssetLoadContext context)
        => Serdes((ListStringSet)existing, s, context);
}