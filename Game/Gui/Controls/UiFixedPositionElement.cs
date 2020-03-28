using System;
using System.Numerics;
using UAlbion.Api;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Core.Visual;

namespace UAlbion.Game.Gui.Controls
{
    public class UiFixedPositionElement<T> : UiElement where T : Enum
    {
        static readonly HandlerSet Handlers = new HandlerSet(
            H<UiFixedPositionElement<T>, WindowResizedEvent>((x,_) => x.Rebuild()),
            H<UiFixedPositionElement<T>, ExchangeDisabledEvent>((x, _) => { x._sprite?.Dispose(); x._sprite = null; }));

        readonly T _id;
        readonly Rectangle _extents;
        SpriteLease _sprite;

        public UiFixedPositionElement(T id, Rectangle extents) : base(Handlers)
        {
            _id = id;
            _extents = extents;
        }

        public override string ToString() => $"{_id} @ {_extents}";
        public override Vector2 GetSize() => new Vector2(_extents.Width, _extents.Height);

        public override void Subscribed()
        {
            if (_sprite == null)
            {
                var assets = Resolve<IAssetManager>();
                var texture = assets.LoadTexture(_id);
                var key = new SpriteKey(texture, DrawLayer.Interface, SpriteKeyFlags.NoTransform | SpriteKeyFlags.NoDepthTest);
                _sprite = Resolve<ISpriteManager>().Borrow(key, 1, this);
            }

            Rebuild();
            base.Subscribed();
        }

        void Rebuild()
        {
            var window = Resolve<IWindowManager>();
            var position = new Vector3(window.UiToNorm(new Vector2(_extents.X, _extents.Y)), 0);
            var size = window.UiToNormRelative(new Vector2(_extents.Width, _extents.Height));

            var instances = _sprite.Access();
            instances[0] = SpriteInstanceData.TopLeft(position, size, _sprite, 0, 0);
        }

        public override int Select(Vector2 uiPosition, Rectangle extents, int order, Action<int, object> registerHitFunc)
        {
            if (extents.Contains((int)uiPosition.X, (int)uiPosition.Y))
                registerHitFunc(order, this);
            return order;
        }
    }
}
