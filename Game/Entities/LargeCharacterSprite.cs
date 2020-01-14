using System;
using System.Collections.Generic;
using System.Numerics;

namespace UAlbion.Game.Entities
{
    public class LargeSpriteAnimations
    {
        public static readonly IDictionary<SpriteAnimation, int[]> Frames = new Dictionary<SpriteAnimation, int[]>
        {
            { SpriteAnimation.WalkN, new[] {  0,  1,  2 } },
            { SpriteAnimation.WalkE, new[] {  3,  4,  5 } },
            { SpriteAnimation.WalkS, new[] {  6,  7,  8 } },
            { SpriteAnimation.WalkW, new[] {  9, 10, 11 } },
            { SpriteAnimation.SitN,  new[] { 12 } },
            { SpriteAnimation.SitE,  new[] { 13 } },
            { SpriteAnimation.SitS,  new[] { 14 } },
            { SpriteAnimation.SitW,  new[] { 15 } },
            { SpriteAnimation.Sleeping, new[] { 16 } },
        };
    }

    public class SmallSpriteAnimations
    {
        public static readonly IDictionary<SpriteAnimation, int[]> Frames = new Dictionary<SpriteAnimation, int[]>
        {
            { SpriteAnimation.WalkN, new[] { 0,  1,  2 } },
            { SpriteAnimation.WalkE, new[] { 3,  4,  5 } },
            { SpriteAnimation.WalkS, new[] { 6,  7,  8 } },
            { SpriteAnimation.WalkW, new[] { 9, 10, 11 } },
        };
    }

    public class LargeCharacterSprite<TSpriteId> : CharacterSprite<TSpriteId, SpriteAnimation>
        where TSpriteId : Enum
    {
        public LargeCharacterSprite(TSpriteId id, Vector2 position) : base(id, position, LargeSpriteAnimations.Frames) { }
    }
}