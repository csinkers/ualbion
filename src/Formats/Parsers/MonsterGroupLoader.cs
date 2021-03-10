using SerdesNet;
using UAlbion.Config;
using UAlbion.Formats.Assets;

namespace UAlbion.Formats.Parsers
{
    public class MonsterGroupLoader : IAssetLoader<MonsterGroup>
    {
        public MonsterGroup Serdes(MonsterGroup existing, AssetInfo info, AssetMapping mapping, ISerializer s)
            => MonsterGroup.Serdes(info?.AssetId.Id ?? 0, existing, mapping, s);

        public object Serdes(object existing, AssetInfo info, AssetMapping mapping, ISerializer s)
            => Serdes(existing as MonsterGroup, info, mapping, s);
    }
}
