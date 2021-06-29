﻿using System.Numerics;
using UAlbion.Api.Visual;
using UAlbion.Core;
using UAlbion.Core.Visual;
using UAlbion.Formats.Assets;

namespace UAlbion.Game.Gui.Controls
{
    public class Divider : UiElement
    {
        readonly CommonColor _color;
        SpriteLease _sprite;
        Vector3 _lastPosition;
        Vector2 _lastSize;

        public Divider(CommonColor color) => _color = color;

        protected override void Unsubscribed()
        {
            _sprite?.Dispose();
            _sprite = null;
        }

        public override Vector2 GetSize() => new(0, 1);

        void UpdateSprite(Vector3 position, Vector2 size, DrawLayer layer)
        {
            var commonColors = Resolve<ICommonColors>();
            if(layer != _sprite?.Key.RenderOrder)
            {
                _sprite?.Dispose();

                var sm = Resolve<ISpriteManager>();
                var key = new SpriteKey(commonColors.BorderTexture, SpriteSampler.Point, layer, SpriteKeyFlags.NoTransform | SpriteKeyFlags.NoDepthTest);
                _sprite = sm.Borrow(key, 1, this);
            }
            else if (_lastPosition == position && _lastSize == size)
                return;

            bool lockWasTaken = false;
            var instances = _sprite.Lock(ref lockWasTaken);
            try
            {
                var region = commonColors.BorderTexture.Regions[(int)commonColors.Palette[_color]];
                instances[0] = new SpriteInstanceData(position, size, region, SpriteFlags.TopLeft);
            }
            finally { _sprite.Unlock(lockWasTaken); }

            _lastPosition = position;
            _lastSize = size;
        }

        public override int Render(Rectangle extents, int order)
        {
            var window = Resolve<IWindowManager>();
            var size = window.UiToNormRelative(extents.Width, extents.Height);
            var position = new Vector3(window.UiToNorm(extents.X, extents.Y), 0);
            UpdateSprite(position, size, (DrawLayer)order);
            return order;
        }
    }
}
