using System;
using SerdesNet;
using UAlbion.Config;
using UAlbion.Formats.Assets;

namespace UAlbion.Formats.Parsers
{
    [AssetLoader(FileFormat.Palette, FileFormat.PaletteCommon)]
    public class PaletteLoader : IAssetLoader
    {
        public object Serdes(object existing, AssetInfo config, AssetMapping mapping, ISerializer s)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));
            if (config == null) throw new ArgumentNullException(nameof(config));
            return new AlbionPalette(s, config);
        }    
    }
}
