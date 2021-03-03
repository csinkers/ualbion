using System;
using System.Collections.Generic;
using UAlbion.Config;
using UAlbion.Core;
using UAlbion.Formats.Assets.Save;

namespace UAlbion.Game.Assets
{
    public interface IModApplier : IComponent
    {
        void LoadMods(IGeneralConfig config, IList<string> mods);
        IModApplier AddAssetPostProcessor(IAssetPostProcessor postProcessor);
        AssetInfo GetAssetInfo(AssetId key, string language);
        object LoadAsset(AssetId id);
        object LoadAsset(AssetId id, string language);
        object LoadAssetCached(AssetId assetId);
        SavedGame LoadSavedGame(string path);
        IReadOnlyDictionary<string, LanguageConfig> Languages { get; }
        void SaveAssets(
            Func<AssetId, string, (object, AssetInfo)> loaderFunc,
            PaletteHints paletteHints,
            ISet<AssetId> ids,
            ISet<AssetType> assetTypes);
    }
}