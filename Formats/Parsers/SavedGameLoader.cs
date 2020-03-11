using System.IO;
using SerdesNet;
using UAlbion.Formats.Assets.Save;
using UAlbion.Formats.Config;

namespace UAlbion.Formats.Parsers
{
    [AssetLoader(FileFormat.SavedGame)]
    public class SavedGameLoader : IAssetLoader<SavedGame>
    {
        public SavedGame Serdes(SavedGame existing, ISerializer s, string name, AssetInfo config) => SavedGame.Serdes(existing, s);
        public object Load(BinaryReader br, long streamLength, string name, AssetInfo config)
            => Serdes(null, new AlbionReader(br, streamLength), name, config);
    }
}
