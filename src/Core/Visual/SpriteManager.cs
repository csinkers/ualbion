using System;
using System.Collections.Generic;
using UAlbion.Api.Eventing;
using UAlbion.Core.Events;

namespace UAlbion.Core.Visual;

public class SpriteManager<TInstance> : ServiceComponent<ISpriteManager<TInstance>>, ISpriteManager<TInstance>
    where TInstance : unmanaged
{
    readonly object _syncRoot = new();
    readonly Dictionary<SpriteKey, SpriteBatch<TInstance>> _sprites = new();
    readonly List<SpriteBatch<TInstance>> _batches = new();
    float _lastCleanup;
    float _totalTime;

    public SpriteManager() => On<EngineUpdateEvent>(OnUpdate);

    public SpriteLease<TInstance> Borrow(SpriteKey key, int count, object owner)
    {
        if (count <= 0) throw new ArgumentOutOfRangeException(nameof(count));
        var factory = Resolve<ICoreFactory>();
        lock (_syncRoot)
        {
            if (!_sprites.TryGetValue(key, out var entry))
            {
                entry = AttachChild(factory.CreateSpriteBatch<TInstance>(key));
                _sprites[key] = entry;
                _batches.Add(entry);
            }

            return entry.Grow(count, owner);
        }
    }

    void OnUpdate(EngineUpdateEvent e)
    {
        _totalTime += e.DeltaSeconds;
        var config = Resolve<ICoreConfigProvider>().Core.Visual.SpriteManager;

        if (_totalTime - _lastCleanup <= config.CacheCheckIntervalSeconds)
            return;

        lock (_syncRoot)
        {
            var spritesToRemove = new List<KeyValuePair<SpriteKey, SpriteBatch<TInstance>>>();
            foreach (var kvp in _sprites)
                if (kvp.Value.ActiveInstances == 0)
                    spritesToRemove.Add(kvp);

            foreach (var kvp in spritesToRemove)
            {
                _sprites.Remove(kvp.Key);
                _batches.Remove(kvp.Value);
                RemoveChild(kvp.Value);
            }
        }
        _lastCleanup = _totalTime;
    }

    public void Collect(List<IRenderable> renderables)
    {
        if (renderables == null) throw new ArgumentNullException(nameof(renderables));
        lock (_syncRoot)
        {
            foreach (var kvp in _batches)
                renderables.Add(kvp);
        }
    }
}