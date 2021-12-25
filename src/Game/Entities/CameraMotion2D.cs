using System;
using System.Linq;
using System.Numerics;
using UAlbion.Api;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Core.Visual;
using UAlbion.Formats.Config;
using UAlbion.Formats.ScriptEvents;
using UAlbion.Game.State;

namespace UAlbion.Game.Entities
{
    public class CameraMotion2D : Component
    {
        readonly OrthographicCamera _camera;
        Vector3 _position;
        Vector3 _velocity;
        bool _locked;

        public CameraMotion2D(OrthographicCamera camera)
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
                if (_velocity.X == 0 && _velocity.Y == 0 && _velocity.Z == 0)
                    return;
                _position += _velocity * e.DeltaSeconds;
            }
            else
            {
                var party = Resolve<IParty>();
                var config = Resolve<GameConfig>().Visual.Camera2D;
                if (map == null || party == null || !party.StatusBarOrder.Any()) return;
                var leader = party.Leader;
                if (leader == null)
                    return;

                var position = leader.GetPosition() * map.TileSize;
                var curPosition2 = new Vector2(_position.X, _position.Y);
                var position2 = new Vector2(position.X, position.Y);

                if ((curPosition2 - position2).LengthSquared() < 0.25f)
                    _position = new Vector3(position2, _position.Z);
                else
                {
                    _position = new Vector3(
                        ApiUtil.Lerp(_position.X, position.X, config.LerpRate * e.DeltaSeconds),
                        ApiUtil.Lerp(_position.Y, position.Y, config.LerpRate * e.DeltaSeconds),
                        map.BaseCameraHeight);
                }
            }

            _camera.Position = _position;
        }
    }
}
