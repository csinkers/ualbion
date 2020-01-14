using System;
using System.Numerics;
using UAlbion.Formats.AssetIds;

namespace UAlbion.Game.Entities
{
    public class SmallPlayerSprite : SmallCharacterSprite<SmallPartyGraphicsId>
    {
        public SmallPlayerSprite(SmallPartyGraphicsId id, Vector2 position) : base(id, position)
        {
            Animation = (SpriteAnimation)new Random().Next((int)SpriteAnimation.WalkW + 1);
        }
    }
}