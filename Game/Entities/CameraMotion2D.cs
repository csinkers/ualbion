using System.Linq;
using System.Numerics;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Game.Events;
using UAlbion.Game.State;

namespace UAlbion.Game.Entities
{
    public class CameraMotion2D : Component
    {
        static readonly HandlerSet Handlers = new HandlerSet(
            H<CameraMotion2D, BeginFrameEvent>((x, e) => x._velocity = Vector2.Zero),
            H<CameraMotion2D, CameraLockEvent>((x, e) => x._locked = true),
            H<CameraMotion2D, CameraUnlockEvent>((x, e) => x._locked = false),
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
            H<CameraMotion2D, EngineUpdateEvent>((x, e) => x.Update(e))
        );

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
                if (map == null || party == null || !party.StatusBarOrder.Any()) return;
                var leader = party[party.Leader];
                var position = leader.GetPosition() * map.TileSize;
                const float lerpRate = 3.0f; // TODO: Data driven
                _position = new Vector2(
                    Util.Lerp(_position.X, position.X, lerpRate * e.DeltaSeconds),
                    Util.Lerp(_position.Y, position.Y, lerpRate * e.DeltaSeconds));
            }

            if (map == null) return;
            _camera.Position = new Vector3(_position, map.BaseCameraHeight);
        }

        readonly OrthographicCamera _camera;
        Vector2 _position;
        Vector2 _velocity;
        bool _locked;

        public CameraMotion2D(OrthographicCamera camera) : base(Handlers)
        {
            _camera = camera;
        }
    }
}