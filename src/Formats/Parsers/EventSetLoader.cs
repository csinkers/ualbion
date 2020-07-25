using System.IO;
using SerdesNet;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Config;

namespace UAlbion.Formats.Parsers
{
    [AssetLoader(FileFormat.EventSet)]
    public class EventSetLoader : IAssetLoader<EventSet>
    {
        public object Load(BinaryReader br, long streamLength, AssetKey key, AssetInfo config)
            => EventSet.Serdes(
                (EventSetId)config.Id,
                null,
                new AlbionReader(br, streamLength));

        public EventSet Serdes(EventSet existing, ISerializer s, AssetKey key, AssetInfo config)
            => EventSet.Serdes((EventSetId)config.Id, existing, s);
    }
}
