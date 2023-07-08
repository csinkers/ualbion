using System;
using SerdesNet;
using UAlbion.Config;
using UAlbion.Formats.Assets;

namespace UAlbion.Formats.Parsers;

public class MonsterGroupLoader : IAssetLoader<MonsterGroup>
{
    public MonsterGroup Serdes(MonsterGroup existing, ISerializer s, AssetLoadContext context)
    {
        if (context == null) throw new ArgumentNullException(nameof(context));
        return MonsterGroup.Serdes(context.AssetId.Id, existing, context.Mapping, s);
    }

    public object Serdes(object existing, ISerializer s, AssetLoadContext context)
        => Serdes(existing as MonsterGroup, s, context);
}
