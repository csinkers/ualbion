using System.IO;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Assets.Flic;
using UAlbion.Formats.Config;

namespace UAlbion.Formats.Parsers
{
    [AssetLoader(FileFormat.FlicVideo)]
    public class FlicLoader : IAssetLoader
    {
        public object Load(BinaryReader br, long streamLength, AssetKey key, AssetInfo config) => new FlicFile(br);
    }
}