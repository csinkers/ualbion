using System.IO;
using UAlbion.Config;
using UAlbion.Formats.Assets.Flic;

namespace UAlbion.Formats.Parsers
{
    [AssetLoader(FileFormat.FlicVideo)]
    public class FlicLoader : IAssetLoader
    {
        public object Load(BinaryReader br, long streamLength, AssetMapping mapping, AssetId id, AssetInfo config) => new FlicFile(br);
    }
}
