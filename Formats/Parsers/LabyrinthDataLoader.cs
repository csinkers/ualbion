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
            => LabyrinthData.Serdes(
                null,
                new AlbionReader(br, streamLength));
    }
}
