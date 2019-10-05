using System;
using System.Collections.Generic;
using System.Linq;
using UAlbion.Core;
using UAlbion.Formats.AssetIds;
using UAlbion.Game.Events;

namespace UAlbion.Game
{
    public class AssetCache : Component
    {
        static readonly IList<Handler> Handlers = new Handler[]
        {
            new Handler<AssetCache, ReloadAssetsEvent>((x, e) =>
            {
                lock (x._syncRoot)
                {
                    x._assetCache.Clear();
                }
            }),
            new Handler<AssetCache, CycleCacheEvent>((x, e) => { x.CycleCacheEvent(); }),
            new Handler<AssetCache, AssetStatsEvent>((x, e) =>
            {
                Console.WriteLine("Asset Statistics:");
                lock (x._syncRoot)
                {
                    foreach (var key in x._assetCache.Keys.OrderBy(y => y.ToString()))
                    {
                        Console.WriteLine("    {0}: {1} items", key, x._assetCache[key].Values.Count);
                    }
                }
            }),
        };

        readonly object _syncRoot = new object();
        IDictionary<AssetType, IDictionary<int, object>> _assetCache = new Dictionary<AssetType, IDictionary<int, object>>();
        IDictionary<AssetType, IDictionary<int, object>> _oldAssetCache = new Dictionary<AssetType, IDictionary<int, object>>();

        public AssetCache() : base(Handlers) { }

        void CycleCacheEvent()
        {
            lock (_syncRoot)
            {
                _oldAssetCache = _assetCache;
                _assetCache = new Dictionary<AssetType, IDictionary<int, object>>();
            }
        }

        public object Get(AssetType type, int id)
        { 
            lock (_syncRoot)
            {
                if (_assetCache.TryGetValue(type, out var typeCache))
                {
                    if (typeCache.TryGetValue(id, out var cachedAsset))
                        return cachedAsset;
                }
                else _assetCache[type] = new Dictionary<int, object>();

                // Check old cache
                if (_oldAssetCache.TryGetValue(type, out var oldTypeCache) && oldTypeCache.TryGetValue(id, out var oldCachedAsset))
                {
                    if (!(oldCachedAsset is Exception))
                    {
                        _assetCache[type][id] = oldCachedAsset;
                        return oldCachedAsset;
                    }
                }
            }

            return null;
        }

        public void Add(object asset, AssetType type, int id)
        {
            lock (_syncRoot)
            {
                _assetCache[type][id] = asset;
            }
        }
    }
}