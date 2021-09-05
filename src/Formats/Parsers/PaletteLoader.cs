using System;
using SerdesNet;
using UAlbion.Api;
using UAlbion.Config;
using UAlbion.Formats.Assets;

namespace UAlbion.Formats.Parsers
{
    public class PaletteLoader : IAssetLoader<AlbionPalette>
    {
        public object Serdes(object existing, AssetInfo info, AssetMapping mapping, ISerializer s, IJsonUtil jsonUtil)
            => Serdes((AlbionPalette)existing, info, mapping, s, jsonUtil);

        public AlbionPalette Serdes(AlbionPalette existing, AssetInfo info, AssetMapping mapping, ISerializer s, IJsonUtil jsonUtil)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));
            if (info == null) throw new ArgumentNullException(nameof(info));
            return AlbionPalette.Serdes(existing, info,  s);
        }    
    }
}
