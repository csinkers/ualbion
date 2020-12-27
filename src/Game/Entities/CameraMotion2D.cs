using System.Linq;
using System.Numerics;
using UAlbion.Api;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Core.Visual;
using UAlbion.Formats.Config;
using UAlbion.Game.Events;
using UAlbion.Game.State;

namespace UAlbion.Game.Entities
{
    public class CameraMotion2D : Component
    {
        readonly OrthographicCamera _camera;
        Vector2 _position;
        Vector2 _velocity;
        bool _locked;

        public CameraMotion2D(OrthographicCamera camera)
        {
            On<EngineUpdateEvent>(Update);
            On<BeginFrameEvent>(e => _velocity = Vector2.Zero);
            On<CameraLockEvent>(e => _locked = true);
            On<CameraUnlockEvent>(e => _locked = false);
            On<CameraJumpEvent>(e =>
            {
                var map = Resolve<IMapManager>().Current;
                if (map == null) return;
                _position = new Vector2(e.X * map.TileSize.X + 0.1f, e.Y * map.TileSize.Y + 0.1f);
                _camera.Position = new Vector3(_position, map.BaseCameraHeight);
            });
            On<CameraMoveEvent>(e =>
            {
                var map = Resolve<IMapManager>().Current;
                if (map == null) return;
                _velocity += new Vector2(e.X * map.TileSize.X, e.Y * map.TileSize.Y);
            });

            _camera = camera;
        }

        void Update(EngineUpdateEvent e)
        {
            var map = Resolve<IMapManager>().Current;
            if (_locked)
            {
                _position += new Vector2(_velocity.X, _velocity.Y) * e.DeltaSeconds;
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
                var position2 = new Vector2(position.X, position.Y);

                if ((_position - position2).LengthSquared() < 0.25f)
                    _position = position2;
                else
                    _position = new Vector2(
                        ApiUtil.Lerp(_position.X, position.X, config.LerpRate * e.DeltaSeconds),
                        ApiUtil.Lerp(_position.Y, position.Y, config.LerpRate * e.DeltaSeconds));
            }

            if (map == null) return;
            _camera.Position = new Vector3(_position, map.BaseCameraHeight);
        }
    }
}
