using System.IO;
using SerdesNet;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Config;

namespace UAlbion.Formats.Parsers
{
    [AssetLoader(FileFormat.MonsterGroup)]
    public class MonsterGroupLoader : IAssetLoader<MonsterGroup>
    {
        public MonsterGroup Serdes(MonsterGroup existing, ISerializer s, AssetKey key, AssetInfo config)
            => MonsterGroup.Serdes(key.Id, existing, s);

        public object Load(BinaryReader br, long streamLength, AssetKey key, AssetInfo config)
            => Serdes(null, new AlbionReader(br, streamLength), key, config);
    }
}
