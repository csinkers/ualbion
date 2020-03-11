using System.Numerics;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Game.Events;

namespace UAlbion.Game.Entities
{
    public class CameraMotion3D : Component
    {
        static readonly HandlerSet Handlers = new HandlerSet(
            H<CameraMotion3D, BeginFrameEvent>((x, e) => x._velocity = Vector3.Zero),
            H<CameraMotion3D, CameraJumpEvent>((x, e) =>
            {
                var map = x.Resolve<IMapManager>().Current;
                if (map == null) return;
                x._camera.Position = new Vector3(e.X * map.TileSize.X, map.BaseCameraHeight, e.Y * map.TileSize.Y);
            }),
            H<CameraMotion3D, CameraMoveEvent>((x, e) =>
            {
                var map = x.Resolve<IMapManager>().Current;
                if (map == null) return;
                x._velocity += new Vector3(e.X, 0, e.Y) * map.TileSize;
            }),
            H<CameraMotion3D, CameraRotateEvent>((x, e) => {
                x._camera.Yaw += e.Yaw;
                x._camera.Pitch += e.Pitch;
            }),
            H<CameraMotion3D, EngineUpdateEvent>((x, e) =>
            {
                if (x._velocity == Vector3.Zero)
                    return;

                Quaternion lookRotation = Quaternion.CreateFromYawPitchRoll(x._camera.Yaw, 0f, 0f);
                x._camera.Position += Vector3.Transform(x._velocity, lookRotation) * e.DeltaSeconds;
            }));

        readonly PerspectiveCamera _camera;
        Vector3 _velocity;

        public CameraMotion3D(PerspectiveCamera camera) : base(Handlers)
        {
            _camera = camera;
        }
    }
}
