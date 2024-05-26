using System;
using System.Collections.Generic;
using UAlbion.Api.Eventing;
using UAlbion.Core.Events;

namespace UAlbion.Core.Visual;

public class BatchManager<TKey, TInstance> : ServiceComponent<IBatchManager<TKey, TInstance>>, IBatchManager<TKey, TInstance>
    where TKey : IBatchKey, IEquatable<TKey>
    where TInstance : unmanaged
{
    public delegate RenderableBatch<TKey, TInstance> BatchFactoryFunc(TKey key, ICoreFactory factory);
    readonly BatchFactoryFunc _factory;
    readonly object _syncRoot = new();
    readonly Dictionary<TKey, RenderableBatch<TKey, TInstance>> _batchLookup = new();
    readonly List<RenderableBatch<TKey, TInstance>> _batchList = new();
    float _lastCleanup;
    float _totalTime;

    public BatchManager(BatchFactoryFunc factory)
    {
        _factory = factory ?? throw new ArgumentNullException(nameof(factory));
        On<EngineUpdateEvent>(OnUpdate);
    }

    public BatchLease<TKey, TInstance> Borrow(TKey key, int count, object owner)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(count);
        lock (_syncRoot)
        {
            if (!_batchLookup.TryGetValue(key, out var entry))
            {
                var coreFactory = Resolve<ICoreFactory>();
                entry = AttachChild(_factory(key, coreFactory));
                _batchLookup[key] = entry;
                _batchList.Add(entry);
            }

            return entry.Grow(count, owner);
        }
    }

    void OnUpdate(EngineUpdateEvent e)
    {
        _totalTime += e.DeltaSeconds;

        if (_totalTime - _lastCleanup <= ReadVar(V.Core.Gfx.SpriteManager.CacheCheckIntervalSeconds))
            return;

        lock (_syncRoot)
        {
            var spritesToRemove = new List<KeyValuePair<TKey, RenderableBatch<TKey, TInstance>>>();
            foreach (var kvp in _batchLookup)
                if (kvp.Value.ActiveInstances == 0)
                    spritesToRemove.Add(kvp);

            foreach (var kvp in spritesToRemove)
            {
                _batchLookup.Remove(kvp.Key);
                _batchList.Remove(kvp.Value);
                RemoveChild(kvp.Value);
            }
        }
        _lastCleanup = _totalTime;
    }

    public void Collect(List<IRenderable> renderables)
    {
        ArgumentNullException.ThrowIfNull(renderables);
        lock (_syncRoot)
        {
            foreach (var kvp in _batchList)
                if (kvp.ActiveInstances > 0)
                    renderables.Add(kvp);
        }
    }
}
