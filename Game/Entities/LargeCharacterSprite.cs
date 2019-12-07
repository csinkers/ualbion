using System;
using System.Collections.Generic;
using System.Numerics;

namespace UAlbion.Game.Entities
{
    public class LargeCharacterSprite<TSpriteId> : CharacterSprite<TSpriteId, LargeSpriteAnimation>
        where TSpriteId : Enum
    {
        static readonly IDictionary<LargeSpriteAnimation, int[]> Frames = new Dictionary<LargeSpriteAnimation, int[]>
        {
            { LargeSpriteAnimation.WalkN, new[] {  0,  1,  2 } },
            { LargeSpriteAnimation.WalkE, new[] {  3,  4,  5 } },
            { LargeSpriteAnimation.WalkS, new[] {  6,  7,  8 } },
            { LargeSpriteAnimation.WalkW, new[] {  9, 10, 11 } },
            { LargeSpriteAnimation.SitN,  new[] { 12 } },
            { LargeSpriteAnimation.SitE,  new[] { 13 } },
            { LargeSpriteAnimation.SitS,  new[] { 14 } },
            { LargeSpriteAnimation.SitW,  new[] { 15 } },
            { LargeSpriteAnimation.UpperBody, new[] { 16 } },
        };

        public LargeCharacterSprite(TSpriteId id, Vector2 position) : base(id, position, Frames)
        {
        }
    }
}