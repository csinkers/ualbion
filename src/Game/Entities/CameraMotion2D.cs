using System;
using System.Linq;
using System.Numerics;
using UAlbion.Api;
using UAlbion.Api.Eventing;
using UAlbion.Core.Events;
using UAlbion.Core.Visual;
using UAlbion.Formats.Config;
using UAlbion.Formats.ScriptEvents;
using UAlbion.Game.State;

namespace UAlbion.Game.Entities;

public class CameraMotion2D : Component
{
    readonly ICamera _camera;
    Vector3 _position;
    Vector3 _velocity;
    bool _locked;

    public CameraMotion2D(ICamera camera)
    {
        _camera = camera ?? throw new ArgumentNullException(nameof(camera));

        On<EngineUpdateEvent>(Update);
        On<BeginFrameEvent>(_ => _velocity = Vector3.Zero);
        On<CameraLockEvent>(_ => _locked = true);
        On<CameraUnlockEvent>(_ => _locked = false);
        On<CameraJumpEvent>(e =>
        {
            var map = TryResolve<IMapManager>()?.Current;
            if (map == null)
            {
                _position = new Vector3(e.X, e.Y, e.Z ?? _camera.Position.Z);
                _camera.Position = _position;
            }
            else
            {
                _position = new Vector3(e.X * map.TileSize.X + 0.1f, e.Y * map.TileSize.Y + 0.1f, e.Z ?? map.BaseCameraHeight);
                _camera.Position = _position;
            }
        });

        On<CameraMoveEvent>(e =>
        {
            var map = TryResolve<IMapManager>()?.Current;
            if (map == null) _velocity += new Vector3(e.X, e.Y, e.Z ?? 0);
            else _velocity += new Vector3(e.X * map.TileSize.X, e.Y * map.TileSize.Y, e.Z ?? 0);
        });
    }

    void Update(EngineUpdateEvent e)
    {
        var map = TryResolve<IMapManager>()?.Current;
        if (map == null)
            _locked = true;

        if (_locked)
        {
            if (_velocity is { X: 0, Y: 0, Z: 0 })
                return;

            _position += _velocity * e.DeltaSeconds;
        }
        else
        {
            var party = Resolve<IParty>();
            if (map == null || party == null || !party.StatusBarOrder.Any())
                return;

            var leader = party.Leader;
            if (leader == null)
                return;

            var tileOffset = new Vector3(
                Var(GameVars.VisualVars.Camera2D.TileOffsetX),
                Var(GameVars.VisualVars.Camera2D.TileOffsetY),
                0);

            var tilePosition = leader.GetPosition() + tileOffset;
            var posXYZ = tilePosition * map.TileSize;
            var curPosXY = new Vector2(_position.X, _position.Y);
            var posXY = new Vector2(posXYZ.X, posXYZ.Y);
            var len2 = (curPosXY - posXY).LengthSquared();

            if (len2 < 0.25f)
            {
                if (len2 < 0.001f) // Stop updating the position if we're close enough
                    return;

                _position = new Vector3(posXY, _position.Z);
            }
            else
            {
                var lerpRate = Var(GameVars.VisualVars.Camera2D.LerpRate);
                _position = new Vector3(
                    ApiUtil.Lerp(_position.X, posXY.X, lerpRate * e.DeltaSeconds),
                    ApiUtil.Lerp(_position.Y, posXY.Y, lerpRate * e.DeltaSeconds),
                    map.BaseCameraHeight);
            }
        }

        _camera.Position = _position;
    }
}