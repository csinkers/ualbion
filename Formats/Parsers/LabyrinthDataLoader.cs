using System.IO;
using UAlbion.Formats.Assets.Labyrinth;
using UAlbion.Formats.Config;

namespace UAlbion.Formats.Parsers
{
    [AssetLoader(FileFormat.LabyrinthData)]
    public class LabyrinthDataLoader : IAssetLoader
    {
        public object Load(BinaryReader br, long streamLength, string name, AssetInfo config)
        {
            var labyrinth = new LabyrinthData();
            LabyrinthData.Serialize(labyrinth, new GenericBinaryReader(br, streamLength), streamLength);
            return labyrinth;
        }
    }
}
