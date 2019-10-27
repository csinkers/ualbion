using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using UAlbion.Api;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Core.Visual;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Assets;
using UAlbion.Game.Events;

namespace UAlbion.Game.Entities
{
    public class SmallNpcSprite : Component
    {
        public enum Animation
        {
            WalkN,
            WalkE,
            WalkS,
            WalkW,
        }

        static readonly IDictionary<Animation, int[]> Frames = new Dictionary<Animation, int[]>
        {
            { Animation.WalkN, new[] { 0,1,2 } },
            { Animation.WalkE, new[] { 3,4,5 } },
            { Animation.WalkS, new[] { 6,7,8 } },
            { Animation.WalkW, new[] { 9,10,11 } },
        };

        static readonly HandlerSet Handlers = new HandlerSet(
            H<SmallNpcSprite, RenderEvent>((x,e) => x.Render(e)),
            H<SmallNpcSprite, SetTileSizeEvent>((x,e) => x._tileSize = new Vector2(e.TileSize.X, e.TileSize.Y)),
            H<SmallNpcSprite, UpdateEvent>((x, e) =>
            {
                var cycle = Frames[x._animation];
                x._frame++;
                if (!cycle.Contains(x._frame))
                    x._frame = cycle[0];
            }));

        readonly SmallNpcId _id;
        readonly MapNpc.Waypoint[] _waypoints;
        Vector2 _position;
        Vector2 _tileSize;
        Animation _animation;
        int _frame;

        public SmallNpcSprite(SmallNpcId id, MapNpc.Waypoint[] waypoints) : base(Handlers)
        {
            _id = id;
            _waypoints = waypoints;
            _position = new Vector2(waypoints[0].X, waypoints[0].Y);
            _animation = (Animation)new Random().Next((int)Animation.WalkW + 1);
        }

        void Render(RenderEvent e)
        {
            var positionLayered = new Vector3(_position * _tileSize, DrawLayer.Characters1.ToZCoordinate(_position.Y));
            var npcSprite = new SpriteDefinition<SmallNpcId>(_id, _frame, positionLayered, (int)DrawLayer.Characters1, 0);
            e.Add(npcSprite);
        }
    }
}
