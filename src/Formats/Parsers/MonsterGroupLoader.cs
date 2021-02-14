using SerdesNet;
using UAlbion.Config;
using UAlbion.Formats.Assets;

namespace UAlbion.Formats.Parsers
{
    public class MonsterGroupLoader : IAssetLoader<MonsterGroup>
    {
        public MonsterGroup Serdes(MonsterGroup existing, AssetInfo config, AssetMapping mapping, ISerializer s)
            => MonsterGroup.Serdes(config?.AssetId.Id ?? 0, existing, mapping, s);

        public object Serdes(object existing, AssetInfo config, AssetMapping mapping, ISerializer s)
            => Serdes(existing as MonsterGroup, config, mapping, s);
    }
}
