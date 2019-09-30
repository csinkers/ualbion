using System.Collections.Generic;
using System.Numerics;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Game.Events;

namespace UAlbion.Game.Entities
{
    public class CameraMotion2D : Component
    {
        static readonly IList<Handler> Handlers = new Handler[]
        {
            new Handler<CameraMotion2D, BeginFrameEvent>((x, e) => x._velocity = Vector2.Zero),
            new Handler<CameraMotion2D, CameraJumpEvent>((x, e) =>
            {
                x._position = new Vector2(e.X * x._tileSize.X, e.Y * x._tileSize.Y);
                x._camera.Position = new Vector3(x._position, x._height);
            }),
            new Handler<CameraMotion2D, CameraMoveEvent>((x, e) => x._velocity += new Vector2(e.X * x._tileSize.X, e.Y * x._tileSize.Y)),
            new Handler<CameraMotion2D, SetTileSizeEvent>((x, e) =>
            {
                x._tileSize = new Vector2(e.TileSize.X, e.TileSize.Y);
                x._height = e.BaseCameraHeight;
            }),
            new Handler<CameraMotion2D, EngineUpdateEvent>((x, e) =>
            {
                x._position += new Vector2(x._velocity.X, x._velocity.Y) * e.DeltaSeconds;
                x._camera.Position = new Vector3(x._position, x._height);
            }),
        };

        readonly OrthographicCamera _camera;
        Vector2 _position;
        Vector2 _velocity;
        Vector2 _tileSize = Vector2.One;
        float _height;

        public CameraMotion2D(OrthographicCamera camera) : base(Handlers)
        {
            _camera = camera;
        }
    }
}