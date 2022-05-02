using SerdesNet;
using UAlbion.Config;
using UAlbion.Formats.Assets;

namespace UAlbion.Formats.Parsers;

public class InterlacedBitmapLoader : IAssetLoader<InterlacedBitmap>
{
    public InterlacedBitmap Serdes(InterlacedBitmap existing, AssetInfo info, ISerializer s, LoaderContext context)
        => InterlacedBitmap.Serdes(existing, s);

    public object Serdes(object existing, AssetInfo info, ISerializer s, LoaderContext context)
        => Serdes(existing as InterlacedBitmap, info, s, context);
}
