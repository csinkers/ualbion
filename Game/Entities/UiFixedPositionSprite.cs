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
    public class UiFixedPositionSprite<T> : UiElement where T : Enum
    {
        static readonly HandlerSet Handlers = new HandlerSet(
            H<UiFixedPositionSprite<T>, WindowResizedEvent>((x,_) => x.Rebuild())
        );

        readonly T _id;
        readonly Rectangle _extents;
        Sprite<T> _sprite;

        public UiFixedPositionSprite(T id, Rectangle extents) : base(Handlers)
        {
            _id = id;
            _extents = extents;
        }

        public override string ToString() => $"{_id} @ {_extents}";
        public override Vector2 GetSize() => new Vector2(_extents.Width, _extents.Height);
        public override void Subscribed() { Rebuild(); base.Subscribed(); }
        void Rebuild()
        {
            var window = Resolve<IWindowManager>();
            var position = new Vector3(window.UiToNorm(new Vector2(_extents.X, _extents.Y)), 0);
            var size = window.UiToNormRelative(new Vector2(_extents.Width, _extents.Height));

            _sprite = 
                new Sprite<T>(
                _id,
            0,
                position,
                (int)DrawLayer.Interface,
                SpriteFlags.NoTransform | SpriteFlags.LeftAligned | SpriteFlags.NoDepthTest,
                size);
        }

        public override int Render(Rectangle extents, int order, Action<IRenderable> addFunc)
        {
            addFunc(_sprite);
            return order;
        }

        public override int Select(Vector2 uiPosition, Rectangle extents, int order, Action<int, object> registerHitFunc)
        {
            if (extents.Contains((int)uiPosition.X, (int)uiPosition.Y))
                registerHitFunc(order, this);
            return order;
        }
    }
}
