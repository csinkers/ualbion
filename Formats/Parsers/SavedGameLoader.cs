using System.IO;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Config;

namespace UAlbion.Formats.Parsers
{
    [AssetLoader(FileFormat.SavedGame)]
    public class SavedGameLoader : IAssetLoader<SavedGame>
    {
        public SavedGame Serdes(SavedGame existing, ISerializer s, string name, AssetInfo config) => SavedGame.Serdes(existing, s);
        public object Load(BinaryReader br, long streamLength, string name, AssetInfo config) => Serdes(null, new GenericBinaryReader(br, streamLength), name, config);
    }
}