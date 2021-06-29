﻿using System;
using System.Numerics;
using UAlbion.Api.Visual;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Core.Visual;
using UAlbion.Formats.Assets;

namespace UAlbion.Game.Gui.Controls
{
    public class UiSpriteElement : UiElement
    {
        SpriteId _id;
        Vector2 _size;
        SpriteLease _sprite;
        Vector3 _lastPosition;
        Vector2 _lastSize;
        int _subId;
        SpriteFlags _flags;
        bool _dirty = true;

        public UiSpriteElement(SpriteId id)
        {
            On<BackendChangedEvent>(_ => _dirty = true);
            Id = id;
        }

        protected override void Subscribed() => _dirty = true;

        protected override void Unsubscribed()
        {
            _sprite?.Dispose();
            _sprite = null;
        }

        public SpriteId Id
        {
            get => _id;
            set
            {
                if (_id == value) return;
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
            if (registerHitFunc == null) throw new ArgumentNullException(nameof(registerHitFunc));
            if (extents.Contains((int)uiPosition.X, (int)uiPosition.Y))
                registerHitFunc(order, this);
            return order;
        }

        public override int Render(Rectangle extents, int order)
        {
            if (!IsSubscribed)
                return order;

            var newOrder = _sprite?.Key.RenderOrder;
            if (newOrder.HasValue && newOrder.Value != (DrawLayer)order)
                _dirty = true;

            UpdateSprite((DrawLayer)order);

            if (_sprite == null)
            {
                _dirty = false;
                return order;
            }

            var window = Resolve<IWindowManager>();
            var position = new Vector3(window.UiToNorm(extents.X, extents.Y), 0);
            var size = window.UiToNormRelative(extents.Width, extents.Height);

            if (!_dirty && _lastPosition == position && _lastSize == size)
                return order;

            _lastPosition = position;
            _lastSize = size;
            _dirty = false;

            bool lockWasTaken = false;
            var instances = _sprite.Lock(ref lockWasTaken);
            try
            {
                instances[0] = new SpriteInstanceData(position, size, _sprite.Key.Texture.Regions[_subId], SpriteFlags.TopLeft | _flags);
            }
            finally { _sprite.Unlock(lockWasTaken); }

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

            if (_id.IsNone)
            {
                _size = Vector2.One;
            }
            else
            {
                var texture = assets.LoadTexture(_id);
                if (texture == null)
                    return;
                var key = new SpriteKey(texture, SpriteSampler.Point, order, SpriteKeyFlags.NoDepthTest | SpriteKeyFlags.NoTransform);
                _sprite = sm.Borrow(key, 1, this);
                _size = texture.Regions[0].Size;
            }
        }
    }
}
