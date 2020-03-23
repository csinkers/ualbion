using System;

namespace UAlbion.Core.Visual
{
    public class WeakSpriteReference
    {
        readonly WeakReference<SpriteLease> _lease;
        readonly MultiSprite _multiSprite;
        readonly int _offset;

        public WeakSpriteReference(MultiSprite multiSprite, SpriteLease lease, int offset)
        {
            _multiSprite = multiSprite;
            _lease = new WeakReference<SpriteLease>(lease);
            _offset = offset;
        }

        public SpriteInstanceData? Data
        {
            get
            {
                if (_multiSprite == null ||
                    _lease == null ||
                    !_lease.TryGetTarget(out var lease) ||
                    lease.Disposed)
                {
                    return null;
                }

                return _multiSprite.Instances[lease.From + _offset];
            }
        }
    }
}