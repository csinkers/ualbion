using System.Collections.Generic;
using System.Numerics;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Core.Visual;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Parsers;

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

        static readonly IList<Handler> Handlers = new Handler[]
        {
            new Handler<SmallNpcSprite, RenderEvent>((x,e)=>x.Render(e)),
        };

        readonly SmallNpcId _id;
        readonly MapNpc.Waypoint[] _waypoints;
        Vector2 _position;
        Animation _animation;
        int _frame;

        public SmallNpcSprite(SmallNpcId id, MapNpc.Waypoint[] waypoints) : base(Handlers)
        {
            _id = id;
            _waypoints = waypoints;
            _position = new Vector2(waypoints[0].X * 16, waypoints[0].Y * 16);
        }

        void Render(RenderEvent e)
        {
            var positionLayered = new Vector3(_position, (255 - _position.Y + (int)DrawLayer.Characters1) / 255.0f);
            var npcSprite = new SpriteDefinition<SmallNpcId>(_id, 0, positionLayered, (int)DrawLayer.Characters1, 0);
            e.Add(npcSprite);
        }
    }
}
