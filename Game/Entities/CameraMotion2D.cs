using System.Numerics;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Game.Events;

namespace UAlbion.Game.Entities
{
    public class CameraMotion2D : Component
    {
        static readonly HandlerSet Handlers = new HandlerSet(
            H<CameraMotion2D, BeginFrameEvent>((x, e) => x._velocity = Vector2.Zero),
            H<CameraMotion2D, CameraJumpEvent>((x, e) =>
            {
                var map = x.Resolve<IMapManager>().Current;
                if (map == null) return;
                x._position = new Vector2(e.X * map.TileSize.X + 0.1f, e.Y * map.TileSize.Y + 0.1f);
                x._camera.Position = new Vector3(x._position, map.BaseCameraHeight);
            }),
            H<CameraMotion2D, CameraMoveEvent>((x, e) =>
            {
                var map = x.Resolve<IMapManager>().Current;
                if (map == null) return;
                x._velocity += new Vector2(e.X * map.TileSize.X, e.Y * map.TileSize.Y);
            }),
            H<CameraMotion2D, EngineUpdateEvent>((x, e) =>
            {
                x._position += new Vector2(x._velocity.X, x._velocity.Y) * e.DeltaSeconds;
                var map = x.Resolve<IMapManager>().Current;
                if (map == null) return;
                x._camera.Position = new Vector3(x._position, map.BaseCameraHeight);
            }));

        readonly OrthographicCamera _camera;
        Vector2 _position;
        Vector2 _velocity;

        public CameraMotion2D(OrthographicCamera camera) : base(Handlers)
        {
            _camera = camera;
        }
    }
}