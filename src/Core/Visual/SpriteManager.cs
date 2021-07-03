using System;
using System.Collections.Generic;

namespace UAlbion.Core.Visual
{
    public class SpriteManager : ServiceComponent<ISpriteManager>, ISpriteManager
    {
        readonly object _syncRoot = new();
        readonly Dictionary<SpriteKey, SpriteBatch> _sprites = new();
        readonly List<SpriteBatch> _batches = new();

        public SpriteLease Borrow(SpriteKey key, int length, object caller)
        {
            if (length <= 0) throw new ArgumentOutOfRangeException(nameof(length));
            var factory = Resolve<ICoreFactory>();
            lock (_syncRoot)
            {
                if (!_sprites.TryGetValue(key, out var entry))
                {
                    entry = AttachChild(factory.CreateSpriteBatch(key));
                    _sprites[key] = entry;
                    _batches.Add(entry);
                }

                return entry.Grow(length, caller);
            }
        }

        public IReadOnlyList<SpriteBatch> Batches => _batches;

        public void Cleanup()
        {
            lock (_syncRoot)
            {
                var spritesToRemove = new List<KeyValuePair<SpriteKey, SpriteBatch>>();
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
        }
    }
}