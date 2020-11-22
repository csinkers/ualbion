using SerdesNet;
using UAlbion.Config;
using UAlbion.Formats.Assets.Save;

namespace UAlbion.Formats.Parsers
{
    [AssetLoader(FileFormat.SavedGame)]
    public class SavedGameLoader : IAssetLoader<SavedGame>
    {
        public SavedGame Serdes(SavedGame existing, AssetInfo config, AssetMapping mapping, ISerializer s)
            => SavedGame.Serdes(existing, mapping, s);

        public object Serdes(object existing, AssetInfo config, AssetMapping mapping, ISerializer s)
            => Serdes(existing as SavedGame, config, mapping, s);
    }
}
