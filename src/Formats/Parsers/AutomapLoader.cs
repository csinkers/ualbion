using SerdesNet;
using UAlbion.Config;
using UAlbion.Formats.Assets;

namespace UAlbion.Formats.Parsers;

public class AutomapLoader : IAssetLoader<Automap>
{
    public Automap Serdes(Automap existing, AssetInfo info, ISerializer s, LoaderContext context) => Automap.Serdes(existing, s);
    public object Serdes(object existing, AssetInfo info, ISerializer s, LoaderContext context)
        => Serdes((Automap)existing, info, s, context);
}
