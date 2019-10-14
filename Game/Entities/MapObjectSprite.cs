using System.Numerics;
using UAlbion.Api;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Core.Visual;
using UAlbion.Formats.AssetIds;
using UAlbion.Game.Events;

namespace UAlbion.Game.Entities
{
    public class MapObjectSprite : Component
    {
        static readonly HandlerSet Handlers = new HandlerSet(
            H<MapObjectSprite, RenderEvent>((x,e) => x.Render(e)),
            H<MapObjectSprite, UpdateEvent>((x, e) => x._frame += e.Frames)
            //H<MapObjectSprite, WorldCoordinateSelectEvent>((x, e) => x.Select(e)),
        );

        readonly DungeonObjectId _id;
        readonly Vector3 _position;
        readonly bool _onFloor;
        readonly Vector2 _size;
        int _frame;
        public Vector3 Normal => Vector3.UnitZ;

        public MapObjectSprite(DungeonObjectId id, Vector3 position, Vector2 size, bool onFloor) : base(Handlers)
        {
            _id = id;
            _position = position;
            _onFloor = onFloor;
            _size = size;
        }

        public override string ToString() => $"{_id} @ {_position} {_size.X}x{_size.Y} {(_onFloor ? "FLOOR" : "")}";

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
            var sprite = new SpriteDefinition<DungeonObjectId>(
                _id,
                _frame,
                _position,
                (int)DrawLayer.Underlay,
                true,
                SpriteFlags.FlipVertical | SpriteFlags.Billboard | (_onFloor ? SpriteFlags.Floor : 0),
                _size);

            e.Add(sprite);
        }
    }
}
