using System;
using SerdesNet;
using UAlbion.Config;
using UAlbion.Formats.Assets;

namespace UAlbion.Formats.Parsers
{
    public class SpellLoader : IAssetLoader<SpellData>
    {
        public SpellData Serdes(SpellData existing, AssetInfo config, AssetMapping mapping, ISerializer s)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));
            if (s == null) throw new ArgumentNullException(nameof(s));
            return SpellData.Serdes(config.AssetId, existing, s);
        }

        public object Serdes(object existing, AssetInfo config, AssetMapping mapping, ISerializer s)
            => Serdes(existing as SpellData, config, mapping, s);
    }
}
