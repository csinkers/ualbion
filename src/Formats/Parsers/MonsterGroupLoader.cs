using SerdesNet;
using UAlbion.Config;
using UAlbion.Formats.Assets;

namespace UAlbion.Formats.Parsers;

public class MonsterGroupLoader : IAssetLoader<MonsterGroup>
{
    public MonsterGroup Serdes(MonsterGroup existing, AssetInfo info, ISerializer s, SerdesContext context)
        => MonsterGroup.Serdes(info?.AssetId.Id ?? 0, existing, context.Mapping, s);

    public object Serdes(object existing, AssetInfo info, ISerializer s, SerdesContext context)
        => Serdes(existing as MonsterGroup, info, s, context);
}
