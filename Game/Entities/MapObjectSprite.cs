using System.Collections.Generic;
using System.Numerics;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Core.Visual;
using UAlbion.Formats.AssetIds;
using UAlbion.Game.Events;

namespace UAlbion.Game.Entities
{
    public class MapObjectSprite : Component
    {
        static readonly IList<Handler> Handlers = new Handler[]
        {
            new Handler<MapObjectSprite, RenderEvent>((x,e) => x.Render(e)),
            new Handler<MapObjectSprite, UpdateEvent>((x, e) => x._frame += e.Frames)
            //new Handler<MapObjectSprite, WorldCoordinateSelectEvent>((x, e) => x.Select(e)),
        };

        readonly DungeonObjectId _id;
        readonly Vector3 _position;
        readonly Vector2 _size;
        int _frame;
        int _frameCount;
        public Vector3 Normal => Vector3.UnitZ;

        public MapObjectSprite(DungeonObjectId id, Vector3 position, Vector2 size) : base(Handlers)
        {
            _id = id;
            _position = position;
            _size = size;
        }

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
                0);

            e.Add(sprite);
        }
    }
}
