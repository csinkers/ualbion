using System;
using System.Collections.Generic;
using SerdesNet;
using UAlbion.Api.Eventing;
using UAlbion.Config;
using UAlbion.Config.Properties;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Ids;

namespace UAlbion.Formats.Parsers;

public class ItemNameCollectorLoader : Component, IAssetLoader<Dictionary<string, ListStringSet>>
{
    public static readonly AssetRangeAssetProperty TargetRange = new("TargetRange");
    public static readonly StringListAssetProperty TargetLanguages = new("TargetLanguages");
    public object Serdes(object existing, ISerdes s, AssetLoadContext context)
        => Serdes((Dictionary<string, ListStringSet>)existing, s, context);

    public Dictionary<string, ListStringSet> Serdes(
        Dictionary<string, ListStringSet> existing,
        ISerdes s,
        AssetLoadContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (existing != null)
            throw new NotSupportedException($"{nameof(StringSetStringLoader)} is read-only");

        var range = context.GetProperty(TargetRange);
        var languages = context.GetProperty(TargetLanguages);

        var results = new Dictionary<string, ListStringSet>();
        var assets = Resolve<IAssetManager>();
        foreach (var language in languages)
        {
            if (!results.TryGetValue(language, out var list))
            {
                list = [];
                results[language] = list;
            }

            var firstId = range.From.Id;
            foreach (var id in range)
            {
                var textId = (TextId)id;
                var value = assets.LoadStringSafe(textId, language);
                list.SetString(new StringId(context.AssetId, (ushort)(id.Id - firstId)), value);
            }
        }

        return results;
    }
}