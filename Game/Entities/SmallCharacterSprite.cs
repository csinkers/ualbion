using System;
using System.Collections.Generic;
using System.Numerics;

namespace UAlbion.Game.Entities
{
    public class SmallCharacterSprite<TSpriteId> : CharacterSprite<TSpriteId, SmallSpriteAnimation>
        where TSpriteId : Enum
    {
        static readonly IDictionary<SmallSpriteAnimation, int[]> Frames = new Dictionary<SmallSpriteAnimation, int[]>
        {
            { SmallSpriteAnimation.WalkN, new[] { 0,  1,  2 } },
            { SmallSpriteAnimation.WalkE, new[] { 3,  4,  5 } },
            { SmallSpriteAnimation.WalkS, new[] { 6,  7,  8 } },
            { SmallSpriteAnimation.WalkW, new[] { 9, 10, 11 } },
        };

        public SmallCharacterSprite(TSpriteId id, Vector2 position) : base(id, position, Frames)
        {
            Animation = (SmallSpriteAnimation)new Random().Next((int)SmallSpriteAnimation.WalkW + 1);
        }
    }
}