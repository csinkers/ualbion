using System;
using UAlbion.Core.Visual;

namespace UAlbion.Core.Veldrid.Sprites
{
    public class WeakSpriteReference : IWeakSpriteReference
    {
        readonly WeakReference<SpriteLease> _lease;
        readonly SpriteBatch _spriteBatch;
        readonly int _offset;

        public WeakSpriteReference(SpriteBatch spriteBatch, SpriteLease lease, int offset)
        {
            _spriteBatch = spriteBatch;
            _lease = new WeakReference<SpriteLease>(lease);
            _offset = offset;
        }

        public SpriteInstanceData? Data
        {
            get
            {
                if (_spriteBatch == null ||
                    _lease == null ||
                    !_lease.TryGetTarget(out var lease) ||
                    lease.Disposed)
                {
                    return null;
                }

                bool lockWasTaken = false;
                var span = lease.Lock(ref lockWasTaken);
                try { return span[_offset]; }
                finally { lease.Unlock(lockWasTaken); }
            }
        }
    }
}