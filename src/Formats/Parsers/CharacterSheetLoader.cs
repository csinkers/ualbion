using System;
using SerdesNet;
using UAlbion.Config;
using UAlbion.Formats.Assets;

namespace UAlbion.Formats.Parsers
{
    [AssetLoader(FileFormat.CharacterData)]
    public class CharacterSheetLoader : IAssetLoader<CharacterSheet>
    {
        public CharacterSheet Serdes(CharacterSheet existing, AssetInfo config, AssetMapping mapping, ISerializer s)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));
            return CharacterSheet.Serdes(config.AssetId, existing, mapping, s);
        }

        public object Serdes(object existing, AssetInfo config, AssetMapping mapping, ISerializer s)
            => Serdes(existing as CharacterSheet, config, mapping, s);
    }
}
