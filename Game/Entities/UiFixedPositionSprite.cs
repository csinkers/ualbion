using System;
using System.Numerics;
using UAlbion.Api;
using UAlbion.Core;
using UAlbion.Core.Visual;
using UAlbion.Game.Gui;
using UAlbion.Game.State;
using Veldrid;

namespace UAlbion.Game.Entities
{
    public class UiFixedPositionSprite<T> : UiElement where T : Enum
    {
        readonly T _id;
        readonly Rectangle _extents;

        public UiFixedPositionSprite(T id, Rectangle extents) : base(null)
        {
            _id = id;
            _extents = extents;
        }

        public override string ToString() => $"{_id} @ {_extents}";
        public override Vector2 GetSize() => new Vector2(_extents.Width, _extents.Height);
        public override void Select(Vector2 uiPosition, Rectangle extents, int order, Action<int, object> registerHitFunc)
        {
            if (!extents.Contains((int)uiPosition.X, (int)uiPosition.Y))
                return;

            registerHitFunc(order, this);
        }

        public override int Render(Rectangle extents, int order, Action<IRenderable> addFunc)
        {
            var window = Resolve<IWindowManager>();
            var state = Resolve<IStateManager>();
            var position = new Vector3(window.UiToNorm(new Vector2(extents.X, extents.Y)), 0);
            var size = window.UiToNormRelative(new Vector2(extents.Width, extents.Height));

            var sprite = 
                new SpriteDefinition<T>(
                _id,
            state.FrameCount,
                position,
                (int)DrawLayer.Interface,
                SpriteFlags.NoTransform | SpriteFlags.LeftAligned | SpriteFlags.NoDepthTest,
                size);

            addFunc(sprite);
            return order;
        }
    }
}