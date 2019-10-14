using System;
using System.Numerics;
using UAlbion.Api;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Core.Visual;
using UAlbion.Game.Events;

namespace UAlbion.Game.Entities
{
    public class ScreenSpaceSprite<T> : Component where T : Enum
    {
        static readonly HandlerSet Handlers = new HandlerSet(
            H<ScreenSpaceSprite<T>, RenderEvent>((x,e) => x.Render(e)),
            H<ScreenSpaceSprite<T>, UpdateEvent>((x, e) => x._frame += e.Frames)
            //H<MapObjectSprite, WorldCoordinateSelectEvent>((x, e) => x.Select(e)),
        );

        readonly T _id;
        readonly Vector3 _position;
        readonly Vector2 _size;
        int _frame;
        public Vector3 Normal => Vector3.UnitZ;

        public ScreenSpaceSprite(T id, Vector2 position, Vector2 size) : base(Handlers)
        {
            _id = id;
            _position = new Vector3(position.X, position.Y, 0.0f);
            _size = size;
        }

        public override string ToString() => $"{_id} @ {_position} {_size.X}x{_size.Y}";

        /*
        void Select(WorldCoordinateSelectEvent e)
        {
            float denominator = Vector3.Dot(Normal, e.Direction);
            if (Math.Abs(denominator) < 0.00001f)
                return;

            var pixelPosition = _position * _tileSize;
            float t = Vector3.Dot(new Vector3(pixelPosition, 0.0f) - e.Origin, Normal) / denominator;
            if (t < 0)
                return;

            var intersectionPoint = e.Origin + t * e.Direction;
            int x = (int)(intersectionPoint.X - pixelPosition.X);
            int y = (int)(intersectionPoint.Y - pixelPosition.Y);

            if (x < 0 || x >= _size.X ||
                y < 0 || y >= _size.Y)
                return;

            e.RegisterHit(t, $"NPC {_id}", this);
        }//*/

        void Render(RenderEvent e)
        {
            var sprite = new SpriteDefinition<T>(
                _id,
                _frame,
                _position,
                (int)DrawLayer.Interface,
                true,
                SpriteFlags.NoTransform,
                _size);

            e.Add(sprite);
        }
    }
}
