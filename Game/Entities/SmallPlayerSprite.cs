using System;
using System.Numerics;
using UAlbion.Formats.AssetIds;

namespace UAlbion.Game.Entities
{
    public class SmallPlayerSprite : SmallCharacterSprite<SmallPartyGraphicsId>
    {
        public SmallPlayerSprite(SmallPartyGraphicsId id, Vector2 position) : base(id, position)
        {
            Animation = (SmallSpriteAnimation)new Random().Next((int)SmallSpriteAnimation.WalkW + 1);
        }
    }
}