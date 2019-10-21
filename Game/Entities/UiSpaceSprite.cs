using System;
using System.Numerics;
using UAlbion.Api;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Core.Visual;
using UAlbion.Game.Events;
using Veldrid;

namespace UAlbion.Game.Entities
{
    public class UiSpaceSprite<T> : Component where T : Enum
    {
        static readonly HandlerSet Handlers = new HandlerSet(
            H<UiSpaceSprite<T>, RenderEvent>((x,e) => x.Render(e)),
            H<UiSpaceSprite<T>, UpdateEvent>((x, e) => x._frame += e.Frames)
            //H<MapObjectSprite, WorldCoordinateSelectEvent>((x, e) => x.Select(e)),
        );

        readonly T _id;
        readonly Rectangle _extents;
        int _frame;

        public UiSpaceSprite(T id, Rectangle extents) : base(Handlers)
        {
            _id = id;
            _extents = extents;
        }

        public override string ToString() => $"{_id} @ {_extents}";

        void Render(RenderEvent e)
        {
            var window = Resolve<IWindowManager>();
            var position = new Vector3(window.UiToNorm(new Vector2(_extents.X, _extents.Y)), 0);
            var size = window.UiToNormRelative(new Vector2(_extents.Width, _extents.Height));

            var sprite = 
                new SpriteDefinition<T>(
                _id,
                _frame,
                position,
                (int)DrawLayer.Interface,
                SpriteFlags.NoTransform | SpriteFlags.LeftAligned | SpriteFlags.NoDepthTest,
                size);

            e.Add(sprite);
        }
    }
}