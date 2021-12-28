using SerdesNet;
using UAlbion.Api;
using UAlbion.Config;
using UAlbion.Formats.Assets;

namespace UAlbion.Formats.Parsers;

public class MonsterGroupLoader : IAssetLoader<MonsterGroup>
{
    public MonsterGroup Serdes(MonsterGroup existing, AssetInfo info, AssetMapping mapping, ISerializer s, IJsonUtil jsonUtil)
        => MonsterGroup.Serdes(info?.AssetId.Id ?? 0, existing, mapping, s);

    public object Serdes(object existing, AssetInfo info, AssetMapping mapping, ISerializer s, IJsonUtil jsonUtil)
        => Serdes(existing as MonsterGroup, info, mapping, s, jsonUtil);
}