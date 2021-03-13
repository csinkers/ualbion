using System;
using System.Numerics;
using UAlbion.Api.Visual;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Core.Visual;
using UAlbion.Formats.Assets;

namespace UAlbion.Game.Gui.Controls
{
    public class UiFixedPositionElement : UiElement
    {
        readonly SpriteId _id;
        readonly Rectangle _extents;
        SpriteLease _sprite;

        public UiFixedPositionElement(SpriteId id, Rectangle extents)
        {
            On<BackendChangedEvent>(_ => Rebuild());
            On<WindowResizedEvent>(_ => Rebuild());
            _id = id;
            _extents = extents;
        }

        public override string ToString() => $"{_id} @ {_extents}";
        public override Vector2 GetSize() => new Vector2(_extents.Width, _extents.Height);

        protected override void Subscribed()
        {
            if (_sprite == null)
            {
                var assets = Resolve<IAssetManager>();
                var texture = assets.LoadTexture(_id);
                var key = new SpriteKey(texture, DrawLayer.Interface, SpriteKeyFlags.NoTransform | SpriteKeyFlags.NoDepthTest);
                _sprite = Resolve<ISpriteManager>().Borrow(key, 1, this);
            }

            Rebuild();
        }

        protected override void Unsubscribed()
        {
            _sprite?.Dispose();
            _sprite = null;
        }

        void Rebuild()
        {
            var window = Resolve<IWindowManager>();
            var position = new Vector3(window.UiToNorm(_extents.X, _extents.Y), 0);
            var size = window.UiToNormRelative(_extents.Width, _extents.Height);

            var instances = _sprite.Access();
            instances[0] = SpriteInstanceData.TopLeft(position, size, _sprite, 0, 0);
        }

        public override int Select(Vector2 uiPosition, Rectangle extents, int order, Action<int, object> registerHitFunc)
        {
            if (registerHitFunc == null) throw new ArgumentNullException(nameof(registerHitFunc));
            if (extents.Contains((int)uiPosition.X, (int)uiPosition.Y))
                registerHitFunc(order, this);
            return order;
        }
    }
}
