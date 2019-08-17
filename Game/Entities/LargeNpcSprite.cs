using System.Collections.Generic;
using System.Numerics;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Core.Objects;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Parsers;

namespace UAlbion.Game.Entities
{
    public class LargeNpcSprite : Component
    {
        public enum Animation
        {
            WalkN,
            WalkE,
            WalkS,
            WalkW,
            SitN,
            SitE,
            SitS,
            SitW,
            UpperBody
        }

        static readonly IDictionary<Animation, int[]> Frames = new Dictionary<Animation, int[]>
        {
            { Animation.WalkN, new[] { 0,1,2 } },
            { Animation.WalkE, new[] { 3,4,5 } },
            { Animation.WalkS, new[] { 6,7,8 } },
            { Animation.WalkW, new[] { 9,10,11 } },
            { Animation.SitN,  new[] { 12 } },
            { Animation.SitE,  new[] { 13 } },
            { Animation.SitS,  new[] { 14 } },
            { Animation.SitW,  new[] { 15 } },
            { Animation.UpperBody, new[] { 16 } },
        };

        static readonly IList<Handler> Handlers = new Handler[]
        {
            new Handler<LargeNpcSprite, RenderEvent>((x,e)=>x.Render(e)),
        };

        readonly LargeNpcId _id;
        readonly MapNpc.Waypoint[] _waypoints;
        Vector2 _position;
        Animation _animation;
        int _frame;

        public LargeNpcSprite(LargeNpcId id, MapNpc.Waypoint[] waypoints) : base(Handlers)
        {
            _id = id;
            _waypoints = waypoints;
            _position = new Vector2(waypoints[0].X * 8, waypoints[0].Y * 8);
        }

        void Render(RenderEvent e)
        {
            var npcSprite = new SpriteDefinition<LargeNpcId>(
                _id,
                0,
                _position,
                (int)DrawLayer.Characters1,
                0);

            e.Add(npcSprite);
        }
    }
}