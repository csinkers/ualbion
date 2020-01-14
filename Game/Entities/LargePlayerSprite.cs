using System;
using System.Numerics;
using UAlbion.Formats.AssetIds;

namespace UAlbion.Game.Entities
{
    public class LargePlayerSprite : LargeCharacterSprite<LargePartyGraphicsId>
    {
        public override string ToString() => $"LNpcSprite {Id} {Animation}";

        public LargePlayerSprite(LargePartyGraphicsId id) : base(id, Vector2.Zero)
        {
            Animation = (SpriteAnimation)new Random().Next((int)SpriteAnimation.Sleeping);
        }
    }
}