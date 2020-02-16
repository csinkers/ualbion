using System.Numerics;
using UAlbion.Api;
using UAlbion.Core;
using UAlbion.Core.Visual;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Assets;

namespace UAlbion.Game.Entities
{
    public class LargeNpc : Component
    {
        static readonly HandlerSet Handlers = new HandlerSet(
            H<LargeNpc, SlowClockEvent>((x, e) => { x._sprite.Frame = e.FrameCount; }));
        readonly MapNpc.Waypoint[] _waypoints;
        readonly MapSprite<LargeNpcId> _sprite;
        public override string ToString() => $"LNpc {_sprite.Id}";

        public LargeNpc(LargeNpcId id, MapNpc.Waypoint[] waypoints) : base(Handlers)
        {
            _waypoints = waypoints;
            _sprite = AttachChild(new MapSprite<LargeNpcId>(id, DrawLayer.Characters1, 0, SpriteFlags.BottomAligned));
        }

        public override void Subscribed()
        {
            _sprite.TilePosition = new Vector3(_waypoints[0].X, _waypoints[0].Y, DrawLayer.Characters1.ToZCoordinate(_waypoints[0].Y));
            base.Subscribed();
        }
    }
 }
 
