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
        static readonly HandlerSet Handlers = new HandlerSet(
            H<AssetCache, ReloadAssetsEvent>((x, e) =>
            {
                lock (x._syncRoot)
                {
                    x._assetCache.Clear();
                    x._oldAssetCache.Clear();
                }
            }),
            H<AssetCache, CycleCacheEvent>((x, e) => { x.CycleCacheEvent(); }),
            H<AssetCache, AssetStatsEvent>((x, e) =>
            {
                Console.WriteLine("Asset Statistics:");
                lock (x._syncRoot)
                {
                    foreach (var key in x._assetCache.Keys.OrderBy(y => y.ToString()))
                    {
                        Console.WriteLine("    {0}: {1} items", key, x._assetCache[key].Values.Count);
                    }
                }
            })
        );

        readonly object _syncRoot = new object();
        IDictionary<AssetType, IDictionary<Tuple<int, GameLanguage>, object>> _assetCache = new Dictionary<AssetType, IDictionary<Tuple<int, GameLanguage>, object>>();
        IDictionary<AssetType, IDictionary<Tuple<int, GameLanguage>, object>> _oldAssetCache = new Dictionary<AssetType, IDictionary<Tuple<int, GameLanguage>, object>>();

        public AssetCache() : base(Handlers) { }

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
