using System;
using SerdesNet;
using UAlbion.Config;
using UAlbion.Config.Properties;
using UAlbion.Formats.Assets;

namespace UAlbion.Formats.Parsers;

public class PaletteLoader : IAssetLoader<AlbionPalette>
{
    public static readonly BoolAssetProperty IsCommon = new("IsCommon"); // bool
    public static readonly StringAssetProperty AnimatedRanges = new("AnimatedRanges"); // string (e.g. "0x1-0xf, 0x12-0x1a")
    public static AssetIdAssetProperty NightPalette { get; }= new("NightPalette");

    public object Serdes(object existing, ISerializer s, AssetLoadContext context)
        => Serdes((AlbionPalette)existing, s, context);

    public AlbionPalette Serdes(AlbionPalette existing, ISerializer s, AssetLoadContext context)
    {
        if (s == null) throw new ArgumentNullException(nameof(s));
        return AlbionPalette.Serdes(existing, context, s);
    }
}
