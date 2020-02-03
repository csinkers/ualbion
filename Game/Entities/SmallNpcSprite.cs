using System.Numerics;
using UAlbion.Api;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Assets;

namespace UAlbion.Game.Entities
{
    public class SmallNpcSprite : SmallCharacterSprite<SmallNpcId>
    {
        readonly MapNpc.Waypoint[] _waypoints;
        public override string ToString() => $"SNpcSprite {Id} Z: {DrawLayer.Characters2.ToDebugZCoordinate(Position.Y)}";

        public SmallNpcSprite(SmallNpcId id, MapNpc.Waypoint[] waypoints) : base(id, new Vector2(waypoints[0].X, waypoints[0].Y))
        {
            _waypoints = waypoints;
        }
    }
}
