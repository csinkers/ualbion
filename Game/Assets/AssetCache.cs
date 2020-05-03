using System;
using System.Collections.Generic;
using System.Linq;
using UAlbion.Core;
using UAlbion.Formats;
using UAlbion.Formats.AssetIds;
using UAlbion.Game.Events;

namespace UAlbion.Game.Assets
{
    public class AssetCache : Component
    {
        readonly object _syncRoot = new object();
        IDictionary<AssetType, IDictionary<Tuple<int, GameLanguage>, object>> _assetCache = new Dictionary<AssetType, IDictionary<Tuple<int, GameLanguage>, object>>();
        IDictionary<AssetType, IDictionary<Tuple<int, GameLanguage>, object>> _oldAssetCache = new Dictionary<AssetType, IDictionary<Tuple<int, GameLanguage>, object>>();

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
                Console.WriteLine("Asset Statistics:");
                lock (_syncRoot)
                {
                    foreach (var key in _assetCache.Keys.OrderBy(y => y.ToString()))
                    {
                        Console.WriteLine("    {0}: {1} items", key, _assetCache[key].Values.Count);
                    }
                }
            });
        }

        void CycleCacheEvent()
        {
            lock (_syncRoot)
            {
                _oldAssetCache = _assetCache;
                _assetCache = new Dictionary<AssetType, IDictionary<Tuple<int, GameLanguage>, object>>();
            }
        }

        public object Get(AssetKey key)
        {
            var subKey = Tuple.Create(key.Id, key.Language);
            lock (_syncRoot)
            {
                if (_assetCache.TryGetValue(key.Type, out var typeCache))
                {
                    if (typeCache.TryGetValue(subKey, out var cachedAsset))
                        return cachedAsset;
                }
                else _assetCache[key.Type] = new Dictionary<Tuple<int, GameLanguage>, object>();

                // Check old cache
                if (_oldAssetCache.TryGetValue(key.Type, out var oldTypeCache) && oldTypeCache.TryGetValue(subKey, out var oldCachedAsset))
                {
                    if (!(oldCachedAsset is Exception))
                    {
                        _assetCache[key.Type][subKey] = oldCachedAsset;
                        return oldCachedAsset;
                    }
                }
            }

            return null;
        }

        public void Add(object asset, AssetKey key)
        {
            var subKey = Tuple.Create(key.Id, key.Language);
            lock (_syncRoot)
            {
                _assetCache[key.Type][subKey] = asset;
            }
        }
    }
}
