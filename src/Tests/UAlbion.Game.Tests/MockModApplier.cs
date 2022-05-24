using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using UAlbion.Api.Eventing;
using UAlbion.Config;
using UAlbion.Formats;
using UAlbion.Formats.Assets.Save;

namespace UAlbion.Game.Tests;

public class MockModApplier : ServiceComponent<IModApplier>, IModApplier
{
    readonly Dictionary<AssetId, object> _assets = new();
    readonly Dictionary<AssetId, AssetInfo> _infos = new();

    public void LoadMods(AssetMapping mapping, IPathResolver pathResolver, IList<string> mods) { }
    public AssetInfo GetAssetInfo(AssetId id, string language) => _infos[id];
    public object LoadAsset(AssetId id) => _assets[id];
    public object LoadAsset(AssetId id, string language) => _assets[id];
    public object LoadAssetCached(AssetId id) => _assets[id];
    public string LoadAssetAnnotated(AssetId id, string language) => throw new NotImplementedException();

    public SavedGame LoadSavedGame(string path) => throw new NotImplementedException();
    public IReadOnlyDictionary<string, LanguageConfig> Languages { get; } 
        = new ReadOnlyDictionary<string, LanguageConfig>(
            new Dictionary<string, LanguageConfig>());

    public IEnumerable<string> ShaderPaths => Array.Empty<string>();
    public void SaveAssets(IModApplier.AssetLoaderDelegate loaderFunc, Action flushCacheFunc, ISet<AssetId> ids, ISet<AssetType> assetTypes, Regex filePattern)
    {
        throw new NotImplementedException();
    }

    public MockModApplier Add(AssetId id, object asset)
    {
        _assets[id] = asset;
        return this;
    }

    public MockModApplier AddInfo(AssetId id, AssetInfo asset)
    {
        _infos[id] = asset;
        return this;
    }
}
