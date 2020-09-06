using System;
using System.IO;
using System.Linq;
using SerdesNet;
using UAlbion.Api;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Config;

namespace UAlbion.Formats.Parsers
{
    [AssetLoader(FileFormat.CharacterData)]
    public class CharacterSheetLoader : IAssetLoader<CharacterSheet>
    {
        public CharacterSheet Serdes(CharacterSheet existing, ISerializer s, AssetKey key, AssetInfo config)
            => CharacterSheet.Serdes(key.AssetId, existing, s);

        public object Load(BinaryReader br, long streamLength, AssetKey key, AssetInfo config)
        {
            var sheet = Serdes(null, new AlbionReader(br, streamLength), key, config);
            return sheet;
        }
    }
}
