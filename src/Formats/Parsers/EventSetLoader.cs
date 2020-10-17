using System;
using System.IO;
using SerdesNet;
using UAlbion.Config;
using UAlbion.Formats.Assets;

namespace UAlbion.Formats.Parsers
{
    [AssetLoader(FileFormat.EventSet)]
    public class EventSetLoader : IAssetLoader<EventSet>
    {
        public object Load(BinaryReader br, long streamLength, AssetMapping mapping, AssetId id, AssetInfo config)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));
            return EventSet.Serdes(
                id,
                null,
                mapping,
                new AlbionReader(br, streamLength));
        }

        public EventSet Serdes(EventSet existing, AssetMapping mapping, ISerializer s, AssetId id, AssetInfo config)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));
            return EventSet.Serdes(id, existing, mapping, s);
        }
    }
}
