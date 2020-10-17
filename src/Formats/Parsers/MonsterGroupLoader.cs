using System.IO;
using SerdesNet;
using UAlbion.Config;
using UAlbion.Formats.Assets;

namespace UAlbion.Formats.Parsers
{
    [AssetLoader(FileFormat.MonsterGroup)]
    public class MonsterGroupLoader : IAssetLoader<MonsterGroup>
    {
        public MonsterGroup Serdes(MonsterGroup existing, AssetMapping mapping, ISerializer s, AssetId id, AssetInfo config)
            => MonsterGroup.Serdes(id.Id, existing, mapping, s);

        public object Load(BinaryReader br, long streamLength, AssetMapping mapping, AssetId id, AssetInfo config)
            => Serdes(null, mapping, new AlbionReader(br, streamLength), id, config);
    }
}
