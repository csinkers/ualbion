using System;
using SerdesNet;
using UAlbion.Api.Eventing;
using UAlbion.Config;
using UAlbion.Config.Properties;
using UAlbion.Formats.Assets;

namespace UAlbion.Formats.Parsers;

public class StringSetCollectorLoader : Component, IAssetLoader<ListStringSet>
{
    public static readonly AssetRangeAssetProperty FileRange = new("FileRange");
    public static readonly AssetRangeAssetProperty TargetRange = new("TargetRange");
    public ListStringSet Serdes(ListStringSet existing, ISerdes s, AssetLoadContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        var targetRange = context.GetProperty(TargetRange);
        var fileRange = context.GetProperty(FileRange);

        if (targetRange.From.Type != fileRange.From.Type)
            throw new FormatException($"The asset types of TargetRange and FileRange must match when using {nameof(StringSetCollectorLoader)} (asset {context.AssetId})");

        if (targetRange.From.Id < fileRange.From.Id)
            throw new FormatException($"The TargetRange must match or be a subset of the FileRange when using {nameof(StringSetCollectorLoader)} (asset {context.AssetId})");

        if (targetRange.To.Id > fileRange.To.Id)
            throw new FormatException($"The TargetRange must match or be a subset of the FileRange when using {nameof(StringSetCollectorLoader)} (asset {context.AssetId})");

        var assets = Resolve<IAssetManager>();
        if (!assets.IsStringDefined(targetRange.From, context.Language))
            return null;

        var result = new ListStringSet(fileRange.To.Id - fileRange.From.Id + 1);
        foreach (var id in fileRange)
        {
            int index = id.Id - fileRange.From.Id;
            while (result.Count <= index)
                result.Add(null);

            if (id >= targetRange.From && id <= targetRange.To)
            {
                string value = assets.LoadStringRaw(id, context.Language);
                result[index] = value;
            }
        }

        return result;
    }

    public object Serdes(object existing, ISerdes s, AssetLoadContext context)
        => Serdes((ListStringSet)existing, s, context);
}