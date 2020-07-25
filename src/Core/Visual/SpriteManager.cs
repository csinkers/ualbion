using System;
using System.Collections.Generic;
using UAlbion.Core.Events;

namespace UAlbion.Core.Visual
{
    public class SpriteManager : ServiceComponent<ISpriteManager>, ISpriteManager
    {
        readonly object _syncRoot = new object();
        readonly IDictionary<SpriteKey, MultiSprite> _sprites = new Dictionary<SpriteKey, MultiSprite>();

        public SpriteManager()
        {
            On<RenderEvent>(Render);
        }

        void Render(RenderEvent renderEvent)
        {
            lock (_syncRoot)
                foreach (var sprite in _sprites)
                    renderEvent.Add(sprite.Value);
        }

        public SpriteLease Borrow(SpriteKey key, int length, object caller)
        {
            if (length <= 0) throw new ArgumentOutOfRangeException(nameof(length));
            lock (_syncRoot)
            {
                if (!_sprites.ContainsKey(key))
                    _sprites[key] = new MultiSprite(key);

                var entry = _sprites[key];
                return entry.Grow(length, caller);
            }
        }

        public void Cleanup()
        {
            lock (_syncRoot)
            {
                var keysToRemove = new List<SpriteKey>();
                foreach (var sprite in _sprites.Values)
                    if (sprite.ActiveInstances == 0)
                        keysToRemove.Add(sprite.Key);

                foreach (var key in keysToRemove)
                    _sprites.Remove(key);
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
    }
}
