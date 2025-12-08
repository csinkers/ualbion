using System;
using System.Numerics;
using UAlbion.Api.Eventing;
using UAlbion.Core.Events;
using UAlbion.Core.Visual;
using UAlbion.Formats.ScriptEvents;
using UAlbion.Game.Events;

namespace UAlbion.Game.Entities;

public class CameraMotion3D : Component
{
    readonly ICamera _camera;
    Vector3 _velocity;

    public CameraMotion3D(ICamera camera)
    {
        _camera = camera ?? throw new ArgumentNullException(nameof(camera));
        On<BeginFrameEvent>(_ => _velocity = Vector3.Zero);
        On<CameraJumpEvent>(e =>
        {
            var map = TryResolve<IMapManager>()?.Current;
            if (map == null)
                return;

            _camera.Position = new Vector3(e.X * map.TileSize.X, map.BaseCameraHeight, e.Y * map.TileSize.Y);
        });

        On<CameraMoveEvent>(e =>
        {
            var map = Resolve<IMapManager>().Current;
            if (map == null)
                return;

            _velocity += new Vector3(e.X, 0, e.Y) * map.TileSize;
        });

        On<CameraRotateEvent>(e =>
        {
            _camera.Yaw += e.Yaw;
            _camera.Pitch += e.Pitch;
        });

        On<EngineUpdateEvent>(OnEngineUpdate);
    }

    void OnEngineUpdate(EngineUpdateEvent e)
    {
        if (_velocity == Vector3.Zero)
            return;

        var lookRotation = Quaternion.CreateFromYawPitchRoll(_camera.Yaw, 0f, 0f);
        _camera.Position += Vector3.Transform(_velocity, lookRotation) * e.DeltaSeconds;
    }
}