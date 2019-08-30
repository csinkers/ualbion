using System.Collections.Generic;
using System.Numerics;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Game.Events;

namespace UAlbion.Game.Entities
{
    public class CameraMotion3D : Component
    {
        static readonly IList<Handler> Handlers = new Handler[]
        {
            new Handler<CameraMotion3D, BeginFrameEvent>((x, e) => x._velocity = Vector3.Zero),
            new Handler<CameraMotion3D, CameraJumpEvent>((x, e) =>
            {
                x._camera.Position = new Vector3(e.X * x._tileSize.X, x._height, e.Y * x._tileSize.Y);
            }),
            new Handler<CameraMotion3D, SetCameraPositionEvent>((x,e) => {}),
            new Handler<CameraMotion3D, CameraMoveEvent>((x, e) => x._velocity += new Vector3(e.X, 0, e.Y) * x._tileSize),
            new Handler<CameraMotion3D, CameraRotateEvent>((x, e) => {
                x._camera.Yaw += e.Yaw;
                x._camera.Pitch += e.Pitch;
            }),
            new Handler<CameraMotion3D, SetTileSizeEvent>((x, e) =>
            {
                x._tileSize = e.TileSize;
                x._height = e.BaseCameraHeight;
            }),
            new Handler<CameraMotion3D, EngineUpdateEvent>((x, e) =>
            {
                if (x._velocity == Vector3.Zero)
                    return;

                Quaternion lookRotation = Quaternion.CreateFromYawPitchRoll(x._camera.Yaw, 0f, 0f);
                x._camera.Position += Vector3.Transform(x._velocity, lookRotation) * e.DeltaSeconds;
            }),
        };

        readonly PerspectiveCamera _camera;
        Vector3 _velocity;
        Vector3 _tileSize;
        float _height;

        public CameraMotion3D(PerspectiveCamera camera) : base(Handlers)
        {
            _camera = camera;
        }
    }
}