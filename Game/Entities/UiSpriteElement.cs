using System;
using System.Numerics;
using UAlbion.Api;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Core.Visual;
using UAlbion.Game.Gui;
using Veldrid;

namespace UAlbion.Game.Entities
{
    public class UiSpriteElement<T> : UiElement where T : struct, Enum
    {
        static readonly HandlerSet Handlers = new HandlerSet(
            H<UiSpriteElement<T>, ExchangeDisabledEvent>((x, _) => { x._sprite?.Dispose(); x._sprite = null; }));

        T? _id;
        Vector2 _size;
        SpriteLease _sprite;
        bool _dirty = true;
        Vector3 _lastPosition;
        Vector2 _lastSize;

        public UiSpriteElement(T id) : base(Handlers)
        {
            Id = id;
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

        int _subId;
        bool _highlighted;
        public int SubId { get => _subId; set { if (_subId == value) return; _subId = value; _dirty = true; } }
        public bool Highlighted { get => _highlighted; set { if (_highlighted == value) return; _highlighted = value; _dirty = true; } }

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
                texture.GetSubImageDetails(0, out var size, out _, out _, out _);
                _sprite = sm.Borrow(key, 1, this);
                _size = size;
            }
        }

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
            if (_sprite?.Key.RenderOrder != (DrawLayer) order)
                _dirty = true;

            UpdateSprite((DrawLayer)order);

            if(_sprite == null)
                return order;

            var window = Resolve<IWindowManager>();
            var position = new Vector3(window.UiToNorm(new Vector2(extents.X, extents.Y)), 0);
            var size = window.UiToNormRelative(new Vector2(extents.Width, extents.Height));

            if (!_dirty && _lastPosition == position && _lastSize == size)
                return order;

            var instances = _sprite.Access();
            instances[0] = SpriteInstanceData.TopLeft(position, size, _sprite, _subId, Highlighted ? SpriteFlags.Highlight : 0);
            _lastPosition = position;
            _lastSize = size;
            _dirty = false;

            return order;
        }
    }
}
