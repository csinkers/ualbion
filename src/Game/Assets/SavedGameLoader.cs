using SerdesNet;
using UAlbion.Api;
using UAlbion.Config;
using UAlbion.Core;
using UAlbion.Formats;
using UAlbion.Formats.Assets.Save;

namespace UAlbion.Game.Assets
{
    public class SavedGameLoader : Component, IAssetLoader<SavedGame>
    {
        public SavedGame Serdes(SavedGame existing, AssetInfo info, AssetMapping mapping, ISerializer s, IJsonUtil jsonUtil)
            => SavedGame.Serdes(existing, mapping, s, Resolve<ISpellManager>());

        public object Serdes(object existing, AssetInfo info, AssetMapping mapping, ISerializer s, IJsonUtil jsonUtil)
            => Serdes(existing as SavedGame, info, mapping, s, jsonUtil);
    }
}