using SerdesNet;
using UAlbion.Api;
using UAlbion.Config;
using UAlbion.Formats.Assets;

namespace UAlbion.Formats.Parsers;

public class InterlacedBitmapLoader : IAssetLoader<InterlacedBitmap>
{
    public InterlacedBitmap Serdes(InterlacedBitmap existing, AssetInfo info, AssetMapping mapping, ISerializer s, IJsonUtil jsonUtil)
        => InterlacedBitmap.Serdes(existing, s);

    public object Serdes(object existing, AssetInfo info, AssetMapping mapping, ISerializer s, IJsonUtil jsonUtil)
        => Serdes(existing as InterlacedBitmap, info, mapping, s, jsonUtil);
}