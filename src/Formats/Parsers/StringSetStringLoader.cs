using System;
using SerdesNet;
using UAlbion.Api.Eventing;
using UAlbion.Config;
using UAlbion.Config.Properties;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Ids;

namespace UAlbion.Formats.Parsers;

public class StringSetStringLoader : Component, IAssetLoader<string>
{
    public static readonly AssetIdAssetProperty<StringSetId> TargetProperty = new("Target", StringSetId.None, x => x);
    public static readonly AssetIdAssetProperty<TextId> FirstIdProperty = new("FirstId", TextId.None, x => x);
    public string Serdes(string existing, ISerializer s, AssetLoadContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (existing != null)
            throw new NotSupportedException($"{nameof(StringSetStringLoader)} is read-only");

        var target = context.GetProperty(TargetProperty);
        var firstId = context.GetProperty(FirstIdProperty);
        if (target == AssetId.None)
            return null;

        var assets = Resolve<IAssetManager>();
        var set = assets.LoadStringSet(target, context.Language);

        if (set == null)
            return null;

        if (context.AssetId.Type != firstId.Type)
            throw new InvalidOperationException($"The FirstId for {context.AssetId} ({firstId}) had a mismatched type");

        int thisId = context.AssetId.Id;
        ushort subId = (ushort)(thisId - firstId.Id);
        var stringId = new StringId(target, subId);
        return set.GetString(stringId);
    }

    public object Serdes(object existing, ISerializer s, AssetLoadContext context)
        => Serdes((string)existing, s, context);
}
