using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UAlbion.Config;
using UAlbion.Core;
using UAlbion.Formats.Assets.Save;
using UAlbion.Game.Assets;

namespace UAlbion.Game.Tests
{
    public class MockModApplier : ServiceComponent<IModApplier>, IModApplier
    {
        readonly Dictionary<AssetId, object> _assets = new Dictionary<AssetId, object>();
        readonly Dictionary<AssetId, AssetInfo> _infos = new Dictionary<AssetId, AssetInfo>();

        public void LoadMods(IGeneralConfig config, IList<string> mods) { }
        public IModApplier AddAssetPostProcessor(IAssetPostProcessor postProcessor) => throw new NotImplementedException();
        public AssetInfo GetAssetInfo(AssetId id, string language) => _infos[id];
        public object LoadAsset(AssetId id) => _assets[id];
        public object LoadAsset(AssetId id, string language) => _assets[id];
        public object LoadAssetCached(AssetId id) => _assets[id];
        public SavedGame LoadSavedGame(string path) => throw new NotImplementedException();
        public IReadOnlyDictionary<string, LanguageConfig> Languages { get; } 
            = new ReadOnlyDictionary<string, LanguageConfig>(
                new Dictionary<string, LanguageConfig>());

        public IEnumerable<string> ShaderPaths => Array.Empty<string>();

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
}