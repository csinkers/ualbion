﻿using SerdesNet;
using UAlbion.Config;
using UAlbion.Formats.Assets.Save;

namespace UAlbion.Formats.Parsers
{
    public class SavedGameLoader : IAssetLoader<SavedGame>
    {
        public SavedGame Serdes(SavedGame existing, AssetInfo info, AssetMapping mapping, ISerializer s)
            => SavedGame.Serdes(existing, mapping, s);

        public object Serdes(object existing, AssetInfo info, AssetMapping mapping, ISerializer s)
            => Serdes(existing as SavedGame, info, mapping, s);
    }
}
