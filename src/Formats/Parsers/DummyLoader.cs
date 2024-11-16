using SerdesNet;
using UAlbion.Config;

namespace UAlbion.Formats.Parsers;

public class DummyLoader : IAssetLoader
{
    public object Serdes(object existing, ISerdes s, AssetLoadContext context)
        => existing ?? new object();
}
