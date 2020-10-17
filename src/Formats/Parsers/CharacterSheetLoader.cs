using System.IO;
using SerdesNet;
using UAlbion.Config;
using UAlbion.Formats.Assets;

namespace UAlbion.Formats.Parsers
{
    [AssetLoader(FileFormat.CharacterData)]
    public class CharacterSheetLoader : IAssetLoader<CharacterSheet>
    {
        public CharacterSheet Serdes(CharacterSheet existing, AssetMapping mapping, ISerializer s, AssetId id, AssetInfo config)
            => CharacterSheet.Serdes(id, existing, mapping, s);

        public object Load(BinaryReader br, long streamLength, AssetMapping mapping, AssetId id, AssetInfo config)
        {
            var sheet = Serdes(null, mapping, new AlbionReader(br, streamLength), id, config);
            return sheet;
        }
    }
}
