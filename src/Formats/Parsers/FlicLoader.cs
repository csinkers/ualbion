using SerdesNet;
using UAlbion.Config;
using UAlbion.Formats.Assets.Flic;

namespace UAlbion.Formats.Parsers;

public class FlicLoader : IAssetLoader
{
    public object Serdes(object existing, AssetInfo info, ISerializer s, LoaderContext context) => new FlicFile(s);
}
