using SerdesNet;
using UAlbion.Api;
using UAlbion.Config;
using UAlbion.Formats.Assets.Flic;

namespace UAlbion.Formats.Parsers
{
    public class FlicLoader : IAssetLoader
    {
        public object Serdes(object existing, AssetInfo info, AssetMapping mapping, ISerializer s, IJsonUtil jsonUtil) => new FlicFile(s);
    }
}
