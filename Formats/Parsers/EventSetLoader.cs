using System.IO;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Config;

namespace UAlbion.Formats.Parsers
{
    [AssetLoader(FileFormat.EventSet)]
    public class EventSetLoader : IAssetLoader
    {
        public object Load(BinaryReader br, long streamLength, string name, AssetInfo config)
        {
            var set = new EventSet();
            EventSet.Translate(set, new GenericBinaryReader(br, streamLength), name, config);
            return set;
        }
    }
}