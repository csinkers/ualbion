using System;
using SerdesNet;
using UAlbion.Config;
using UAlbion.Formats.Assets;

namespace UAlbion.Formats.Parsers
{
    public class PaletteLoader : IAssetLoader<AlbionPalette>
    {
        public object Serdes(object existing, AssetInfo info, AssetMapping mapping, ISerializer s)
            => Serdes((AlbionPalette)existing, info, mapping, s);

        public AlbionPalette Serdes(AlbionPalette existing, AssetInfo info, AssetMapping mapping, ISerializer s)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));
            if (info == null) throw new ArgumentNullException(nameof(info));
            return AlbionPalette.Serdes(existing, info,  s);
        }    
    }
}
