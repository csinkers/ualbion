using SerdesNet;
using UAlbion.Config;
using UAlbion.Formats.Assets;

namespace UAlbion.Formats.Parsers;

public class AutomapLoader : IAssetLoader<Automap>
{
    public Automap Serdes(Automap existing, ISerdes s, AssetLoadContext context) 
        => Automap.Serdes(existing, s);
    public object Serdes(object existing, ISerdes s, AssetLoadContext context)
        => Serdes((Automap)existing, s, context);
}
