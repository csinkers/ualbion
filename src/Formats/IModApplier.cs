using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UAlbion.Api.Eventing;
using UAlbion.Config;
using UAlbion.Formats.Assets.Save;

namespace UAlbion.Formats;

public interface IModApplier : IComponent
{
    IReadOnlyDictionary<string, LanguageConfig> Languages { get; }
    IEnumerable<string> ShaderPaths { get; }

    void LoadMods(AssetMapping mapping, IPathResolver pathResolver, IReadOnlyList<string> mods);
    SavedGame LoadSavedGame(string path);
    AssetNode GetAssetInfo(AssetId key, string language = null);

    object LoadAsset(AssetId id, string language = null);
    object LoadAssetCached(AssetId id, string language = null);
    string LoadAssetAnnotated(AssetId id, string language = null);

    AssetLoadResult LoadAssetAndNode(AssetId assetId, string language = null);


    void SaveAssets(
        AssetLoaderMethod loaderFunc,
        Action flushCacheFunc,
        ISet<AssetId> ids,
        ISet<AssetType> assetTypes,
        string[] languages,
        Regex filePattern);
}
