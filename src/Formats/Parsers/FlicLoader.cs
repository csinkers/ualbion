using SerdesNet;
using UAlbion.Config;
using UAlbion.Formats.Assets.Flic;

namespace UAlbion.Formats.Parsers;

public class FlicLoader : IAssetLoader
{
    public object Serdes(object existing, ISerializer s, AssetLoadContext context) => new FlicFile(s);
}
