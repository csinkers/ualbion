using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UAlbion.Api.Eventing;
using UAlbion.Config;
using UAlbion.Formats.Assets.Save;

namespace UAlbion.Formats;

public interface IModApplier : IComponent
{
    void LoadMods(AssetMapping mapping, IPathResolver pathResolver, IReadOnlyList<string> mods);
    AssetNode GetAssetInfo(AssetId key, string language);
    object LoadAsset(AssetId id);
    object LoadAsset(AssetId id, string language);
    object LoadAssetCached(AssetId assetId);
    string LoadAssetAnnotated(AssetId id, string language);
    SavedGame LoadSavedGame(string path);
    IReadOnlyDictionary<string, LanguageConfig> Languages { get; }
    IEnumerable<string> ShaderPaths { get; }

    delegate object AssetLoader(AssetId id, string language);
    void SaveAssets(
        AssetLoader loaderFunc,
        Action flushCacheFunc,
        ISet<AssetId> ids,
        ISet<AssetType> assetTypes,
        string[] languages,
        Regex filePattern);
}
