using System;
using SerdesNet;
using UAlbion.Config;
using UAlbion.Formats.Assets;

namespace UAlbion.Formats.Parsers;

public class PaletteLoader : IAssetLoader<AlbionPalette>
{
    public object Serdes(object existing, AssetInfo info, ISerializer s, LoaderContext context)
        => Serdes((AlbionPalette)existing, info, s, context);

    public AlbionPalette Serdes(AlbionPalette existing, AssetInfo info, ISerializer s, LoaderContext context)
    {
        if (s == null) throw new ArgumentNullException(nameof(s));
        if (info == null) throw new ArgumentNullException(nameof(info));
        return AlbionPalette.Serdes(existing, info,  s);
    }    
}
