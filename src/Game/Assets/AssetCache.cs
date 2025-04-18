﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using UAlbion.Api.Eventing;
using UAlbion.Config;
using UAlbion.Formats;
using UAlbion.Game.Events;
using static System.FormattableString;

namespace UAlbion.Game.Assets;

public class AssetCache : Component
{
    readonly Lock _syncRoot = new();

    class Entry
    {
        public WeakReference Weak;
        public AssetLoadResult Strong;
        public DateTime LastAccessed;
    }

    readonly Dictionary<string, Dictionary<AssetId, Entry>> _langCache = [];
    readonly Dictionary<AssetId, Entry> _cache = [];

    public AssetCache()
    {
        On<CycleCacheEvent>(_ =>
        {
            lock (_syncRoot)
            {
                static void Cycle(Dictionary<AssetId, Entry> cache, DateTime threshold)
                {

                    foreach (var key in cache.Keys.ToList())
                    {
                        var entry = cache[key];
                        if (entry.Strong != null)
                        {
                            if (entry.LastAccessed > threshold)
                                continue;

                            entry.Strong = null;
                        }

                        if (entry.Strong == null && entry.Weak.Target == null)
                            cache.Remove(key);
                    }
                }

                var utcNow = DateTime.UtcNow;
                var threshold = utcNow.AddSeconds(30);

                Cycle(_cache, threshold);

                List<string> emptyLanguages = null;
                foreach (var kvp in _langCache)
                {
                    Cycle(kvp.Value, threshold);
                    if (kvp.Value.Count == 0)
                    {
                        emptyLanguages ??= [];
                        emptyLanguages.Add(kvp.Key);
                    }
                }

                if (emptyLanguages != null)
                    foreach (var lang in emptyLanguages)
                        _langCache.Remove(lang);
            }
        });

        On<ReloadAssetsEvent>(_ =>
        {
            lock (_syncRoot)
            {
                _cache.Clear();
                _langCache.Clear();
            }
        });

        On<AssetUpdatedEvent>(e =>
        {
            lock (_syncRoot)
            {
                _cache.Remove(e.Id);
                foreach (var kvp in _langCache)
                    kvp.Value.Remove(e.Id);
            }
        });

        On<AssetStatsEvent>(_ =>
        {
            var sb = new StringBuilder();
            sb.AppendLine("Asset Statistics:");

            lock (_syncRoot)
            {
                static void Dump(StringBuilder sb2, Dictionary<AssetId, Entry> cache, string lang)
                {
                    var countByType = cache.Keys
                        .GroupBy(x => x.Type)
                        .Select(x => (x.Key, x.Count()))
                        .OrderBy(x => x.Key.ToString());

                    if (lang == null)
                    {
                        foreach (var (type, count) in countByType)
                            sb2.AppendLine(Invariant($"    {type}: {count} items"));
                    }
                    else
                    {
                        foreach (var (type, count) in countByType)
                            sb2.AppendLine(Invariant($"    {type} ({lang}): {count} items"));
                    }
                }

                Dump(sb, _cache, "");
                foreach (var kvp in _langCache)
                    Dump(sb, kvp.Value, kvp.Key);
            }
            Info(sb.ToString());
        });
    }

    Dictionary<AssetId, Entry> GetCache(string language)
    {
        var cache = _cache;
        if (language != null)
            _langCache.TryGetValue(language, out cache);

        return cache;
    }

    Dictionary<AssetId, Entry> GetOrAddCache(string language)
    {
        var cache = _cache;
        if (language != null)
        {
            if (!_langCache.TryGetValue(language, out cache))
            {
                cache = [];
                _langCache[language] = cache;
            }
        }

        return cache;
    }

    public AssetLoadResult Get(AssetId key, string language)
    {
        lock (_syncRoot)
        {
            var cache = GetCache(language);
            if (cache == null || !_cache.TryGetValue(key, out var entry))
                return null;

            entry.LastAccessed = DateTime.UtcNow;
            return entry.Strong ?? (AssetLoadResult)entry.Weak.Target;
        }
    }

    public void Add(AssetId id, string language, object asset, AssetNode node)
    {
        lock (_syncRoot)
        {
            var cache = GetOrAddCache(language);
            var loadResult = new AssetLoadResult(id, asset, node);
            cache[id] = new Entry
            {
                LastAccessed = DateTime.UtcNow,
                Strong = loadResult,
                Weak = new WeakReference(loadResult)
            };
        }
    }
}