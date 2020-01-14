using System;
using System.Numerics;

namespace UAlbion.Game.Entities
{
    public class SmallCharacterSprite<TSpriteId> : CharacterSprite<TSpriteId, SpriteAnimation>
        where TSpriteId : Enum
    {
        public SmallCharacterSprite(TSpriteId id, Vector2 position) : base(id, position, SmallSpriteAnimations.Frames)
        {
            Animation = (SpriteAnimation)new Random().Next((int)SpriteAnimation.WalkW + 1);
        }
    }
}