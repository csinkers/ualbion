using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UAlbion.Config;
using UAlbion.Core;
using UAlbion.Game.Events;

namespace UAlbion.Game.Assets
{
    public class AssetCache : Component
    {
        readonly object _syncRoot = new object();
        IDictionary<AssetId, object> _assetCache = new Dictionary<AssetId, object>();
        IDictionary<AssetId, object> _oldAssetCache = new Dictionary<AssetId, object>();

        public AssetCache()
        {
            On<CycleCacheEvent>(e => CycleCacheEvent());
            On<ReloadAssetsEvent>(e =>
            {
                lock (_syncRoot)
                {
                    _assetCache.Clear();
                    _oldAssetCache.Clear();
                }
            });
            On<AssetStatsEvent>(e =>
            {
                var sb = new StringBuilder();
                sb.AppendLine("Asset Statistics:");
                lock (_syncRoot)
                {
                    var countByType = _assetCache.Keys
                        .GroupBy(x => x.Type)
                        .Select(x => (x.Key, x.Count()))
                        .OrderBy(x => x.Key.ToString());

                    foreach(var (type, count) in countByType)
                        sb.AppendLine($"    {type}: {count} items");
                }
                Info(sb.ToString());
            });
        }

        void CycleCacheEvent()
        {
            lock (_syncRoot)
            {
                _oldAssetCache = _assetCache;
                _assetCache = new Dictionary<AssetId, object>();
            }
        }

        public object Get(AssetId key)
        {
            lock (_syncRoot)
            {
                if (_assetCache.TryGetValue(key, out var cachedAsset))
                    return cachedAsset;

                // Check old cache
                if (_oldAssetCache.TryGetValue(key, out var oldCachedAsset) && !(oldCachedAsset is Exception))
                {
                    _assetCache[key] = oldCachedAsset;
                    return oldCachedAsset;
                }
            }

            return null;
        }

        public void Add(object asset, AssetId key)
        {
            lock (_syncRoot)
            {
                _assetCache[key] = asset;
            }
        }
    }
}
