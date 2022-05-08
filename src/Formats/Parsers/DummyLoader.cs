using SerdesNet;
using UAlbion.Config;

namespace UAlbion.Formats.Parsers;

public class DummyLoader : IAssetLoader
{
    public object Serdes(object existing, AssetInfo info, ISerializer s, SerdesContext context)
        => existing ?? new object();
}
