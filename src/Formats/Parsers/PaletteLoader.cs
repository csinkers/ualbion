using System;
using SerdesNet;
using UAlbion.Config;
using UAlbion.Formats.Assets;

namespace UAlbion.Formats.Parsers
{
    public class PaletteLoader : IAssetLoader<AlbionPalette>
    {
        public object Serdes(object existing, AssetInfo config, AssetMapping mapping, ISerializer s)
            => Serdes((AlbionPalette)existing, config, mapping, s);

        public AlbionPalette Serdes(AlbionPalette existing, AssetInfo config, AssetMapping mapping, ISerializer s)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));
            if (config == null) throw new ArgumentNullException(nameof(config));
            return AlbionPalette.Serdes(existing, config,  s);
        }    
    }
}
