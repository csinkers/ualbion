using System;
using System.IO;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Assets.Labyrinth;
using UAlbion.Formats.Config;

namespace UAlbion.Formats.Parsers
{
    [AssetLoader(FileFormat.LabyrinthData)]
    public class LabyrinthDataLoader : IAssetLoader
    {
        public object Load(BinaryReader br, long streamLength, AssetKey key, AssetInfo config)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));
            return LabyrinthData.Serdes(config.Id, null, new AlbionReader(br, streamLength));
        }
    }
}
