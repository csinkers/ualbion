using SerdesNet;
using UAlbion.Config;
using UAlbion.Formats.Assets;

namespace UAlbion.Formats.Parsers
{
    public class InterlacedBitmapLoader : IAssetLoader<InterlacedBitmap>
    {
        public InterlacedBitmap Serdes(InterlacedBitmap existing, AssetInfo config, AssetMapping mapping, ISerializer s)
            => InterlacedBitmap.Serdes(existing, s);

        public object Serdes(object existing, AssetInfo config, AssetMapping mapping, ISerializer s)
            => Serdes(existing as InterlacedBitmap, config, mapping, s);
    }
}
