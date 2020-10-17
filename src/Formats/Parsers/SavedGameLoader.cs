using System.IO;
using SerdesNet;
using UAlbion.Config;
using UAlbion.Formats.Assets.Save;

namespace UAlbion.Formats.Parsers
{
    [AssetLoader(FileFormat.SavedGame)]
    public class SavedGameLoader : IAssetLoader<SavedGame>
    {
        public SavedGame Serdes(SavedGame existing, AssetMapping mapping, ISerializer s, AssetId id, AssetInfo config) => SavedGame.Serdes(existing, mapping, s);
        public object Load(BinaryReader br, long streamLength, AssetMapping mapping, AssetId id, AssetInfo config)
            => Serdes(null, mapping, new AlbionReader(br, streamLength), id, config);
    }
}
