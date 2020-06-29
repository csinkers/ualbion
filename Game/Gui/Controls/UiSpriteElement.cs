using System;
using System.Numerics;
using UAlbion.Api;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Core.Visual;

namespace UAlbion.Game.Gui.Controls
{
    public class UiSpriteElement<T> : UiElement where T : struct
    {
        T? _id;
        Vector2 _size;
        SpriteLease _sprite;
        Vector3 _lastPosition;
        Vector2 _lastSize;
        int _subId;
        SpriteFlags _flags;
        bool _dirty = true;

        public UiSpriteElement(T id)
        {
            On<BackendChangedEvent>(_ => _dirty = true);
            Id = id;
        }

        protected override void Unsubscribed()
        {
            _sprite?.Dispose();
            _sprite = null;
        }

        public T Id
        {
            get => _id ?? (T)(object)0;
            set
            {
                int existing = _id == null ? -1 : Convert.ToInt32(_id.Value);
                int newValue = Convert.ToInt32(value);

                if (existing == newValue) return;
                _id = value;
                _dirty = true;
            }
        }

        public int SubId { get => _subId; set { if (_subId == value) return; _subId = value; _dirty = true; } }
        public SpriteFlags Flags { get => _flags; set { if (_flags == value) return; _flags = value; _dirty = true; } }

        public override string ToString() => $"UiSpriteElem {_id}";
        public override Vector2 GetSize() => _size;
        public override int Select(Vector2 uiPosition, Rectangle extents, int order, Action<int, object> registerHitFunc)
        {
            if (extents.Contains((int)uiPosition.X, (int)uiPosition.Y))
                registerHitFunc(order, this);
            return order;
        }

        public override int Render(Rectangle extents, int order)
        {
            if (!IsSubscribed)
                return order;

            if (_sprite?.Key.RenderOrder != (DrawLayer)order)
                _dirty = true;

            UpdateSprite((DrawLayer)order);

            if(_sprite == null)
                return order;

            var window = Resolve<IWindowManager>();
            var position = new Vector3(window.UiToNorm(extents.X, extents.Y), 0);
            var size = window.UiToNormRelative(extents.Width, extents.Height);

            if (!_dirty && _lastPosition == position && _lastSize == size)
                return order;

            var instances = _sprite.Access();
            instances[0] = SpriteInstanceData.TopLeft(position, size, _sprite, _subId, _flags);
            _lastPosition = position;
            _lastSize = size;
            _dirty = false;

            return order;
        }

        void UpdateSprite(DrawLayer order)
        {
            if (Exchange == null || !_dirty)
                return;

            var assets = Resolve<IAssetManager>();
            var sm = Resolve<ISpriteManager>();

            _sprite?.Dispose();
            _sprite = null;

            if (_id == null)
            {
                _size = Vector2.One;
            }
            else
            {
                var texture = assets.LoadTexture(_id.Value);
                if (texture == null)
                    return;
                var key = new SpriteKey(texture, order, SpriteKeyFlags.NoDepthTest | SpriteKeyFlags.NoTransform);
                _sprite = sm.Borrow(key, 1, this);
                _size = texture.GetSubImageDetails(0).Size;
            }
        }
    }
}
