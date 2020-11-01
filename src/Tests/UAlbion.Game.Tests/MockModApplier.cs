using System;
using System.Collections.Generic;
using UAlbion.Config;
using UAlbion.Core;
using UAlbion.Formats;
using UAlbion.Formats.Assets.Save;
using UAlbion.Game.Assets;

namespace UAlbion.Game.Tests
{
    public class MockModApplier : ServiceComponent<IModApplier>, IModApplier
    {
        readonly Dictionary<AssetId, object> _assets = new Dictionary<AssetId, object>();
        readonly Dictionary<AssetId, AssetInfo> _infos = new Dictionary<AssetId, AssetInfo>();

        public MockModApplier(GameLanguage language)
        {
            Language = language;
        }

        public GameLanguage Language { get; set; }

        public AssetInfo GetAssetInfo(AssetId id) => _infos[id];
        public object LoadAsset(AssetId id) => _assets[id];
        public object LoadAssetCached(AssetId id) => _assets[id];
        public SavedGame LoadSavedGame(string path) => throw new NotImplementedException();

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