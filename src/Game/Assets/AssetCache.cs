using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UAlbion.Api.Eventing;
using UAlbion.Config;
using UAlbion.Game.Events;
using static System.FormattableString;

namespace UAlbion.Game.Assets;

public class AssetCache : Component
{
    readonly object _syncRoot = new();
    readonly Dictionary<AssetId, WeakReference> _cache = new();

    public AssetCache()
    {
        On<CycleCacheEvent>(_ =>
        {
            lock (_syncRoot)
            {
                foreach (var key in _cache.Keys.ToList())
                {
                    var weakRef = _cache[key];
                    if (weakRef.Target == null)
                        _cache.Remove(key);
                }
            }

        });
        On<ReloadAssetsEvent>(_ => { lock (_syncRoot) { _cache.Clear(); } });
        On<AssetUpdatedEvent>(e => { lock (_syncRoot) { _cache.Remove(e.Id); } });
        On<AssetStatsEvent>(_ =>
        {
            var sb = new StringBuilder();
            sb.AppendLine("Asset Statistics:");
            lock (_syncRoot)
            {
                var countByType = _cache.Keys
                    .GroupBy(x => x.Type)
                    .Select(x => (x.Key, x.Count()))
                    .OrderBy(x => x.Key.ToString());

                foreach(var (type, count) in countByType)
                    sb.AppendLine(Invariant($"    {type}: {count} items"));
            }
            Info(sb.ToString());
        });
    }

    public object Get(AssetId key)
    {
        lock (_syncRoot)
        {
            return _cache.TryGetValue(key, out var cachedAsset) ? cachedAsset.Target : null;
        }
    }

    public void Add(object asset, AssetId key)
    {
        lock (_syncRoot)
        {
            _cache[key] = new WeakReference(asset);
        }
    }
}