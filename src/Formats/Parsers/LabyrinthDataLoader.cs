using System;
using System.IO;
using UAlbion.Config;
using UAlbion.Formats.Assets.Labyrinth;

namespace UAlbion.Formats.Parsers
{
    [AssetLoader(FileFormat.LabyrinthData)]
    public class LabyrinthDataLoader : IAssetLoader
    {
        public object Load(BinaryReader br, long streamLength, AssetMapping mapping, AssetId id, AssetInfo config)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));
            return LabyrinthData.Serdes(config.Id, null, mapping, new AlbionReader(br, streamLength));
        }
    }
}
