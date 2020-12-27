using System;
using System.Collections.Generic;

namespace UAlbion.Core.Visual
{
    public class SpriteManager : ServiceComponent<ISpriteManager>, ISpriteManager
    {
        readonly object _syncRoot = new object();
        readonly IDictionary<SpriteKey, MultiSprite> _sprites = new Dictionary<SpriteKey, MultiSprite>();

        public SpriteLease Borrow(SpriteKey key, int length, object caller)
        {
            if (length <= 0) throw new ArgumentOutOfRangeException(nameof(length));
            lock (_syncRoot)
            {
                if (!_sprites.TryGetValue(key, out var entry))
                {
                    entry = new MultiSprite(key);
                    Resolve<IEngine>()?.RegisterRenderable(entry);
                    _sprites[key] = entry;
                }

                return entry.Grow(length, caller);
            }
        }

        public void Cleanup()
        {
            lock (_syncRoot)
            {
                var spritesToRemove = new List<KeyValuePair<SpriteKey, MultiSprite>>();
                foreach (var kvp in _sprites)
                    if (kvp.Value.ActiveInstances == 0)
                        spritesToRemove.Add(kvp);

                foreach (var kvp in spritesToRemove)
                {
                    Resolve<IEngine>()?.UnregisterRenderable(kvp.Value);
                    _sprites.Remove(kvp.Key);
                }
            }
        }

        public WeakSpriteReference MakeWeakReference(SpriteLease lease, int index)
        {
            lock (_syncRoot)
            {
                if(lease == null)
                    return new WeakSpriteReference(null, null, 0);
                _sprites.TryGetValue(lease.Key, out var entry);
                return new WeakSpriteReference(entry, lease, index);
            }
        }

        protected override void Subscribed()
        {
            lock (_syncRoot)
                foreach (var sprite in _sprites)
                    Resolve<IEngine>()?.RegisterRenderable(sprite.Value);
            base.Subscribed();
        }
        protected override void Unsubscribed()
        {
            lock (_syncRoot)
                foreach (var sprite in _sprites)
                    Resolve<IEngine>()?.UnregisterRenderable(sprite.Value);
            base.Unsubscribed();
        }
    }
}
