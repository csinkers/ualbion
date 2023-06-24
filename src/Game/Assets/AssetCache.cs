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

    class Entry
    {
        public WeakReference Weak;
        public object Strong;
        public DateTime LastAccessed;
    }

    readonly Dictionary<AssetId, Entry> _cache = new();

    public AssetCache()
    {
        On<CycleCacheEvent>(_ =>
        {
            lock (_syncRoot)
            {
                var utcNow = DateTime.UtcNow;
                var threshold = utcNow.AddSeconds(30);

                foreach (var key in _cache.Keys.ToList())
                {
                    var entry = _cache[key];
                    if (entry.Strong != null)
                    {
                        if (entry.LastAccessed > threshold)
                            continue;

                        entry.Strong = null;
                    }

                    if (entry.Strong == null && entry.Weak.Target == null)
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
            if (!_cache.TryGetValue(key, out var entry))
                return null;

            entry.LastAccessed = DateTime.UtcNow;
            return entry.Strong ?? entry.Weak.Target;
        }
    }

    public void Add(object asset, AssetId key)
    {
        lock (_syncRoot)
        {
            _cache[key] = new Entry
            {
                LastAccessed = DateTime.UtcNow,
                Strong = asset,
                Weak = new WeakReference(asset),
            };
        }
    }
}