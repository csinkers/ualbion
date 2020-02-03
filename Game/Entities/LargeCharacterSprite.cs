using System;
using System.Numerics;

namespace UAlbion.Game.Entities
{
    public class LargeCharacterSprite<TSpriteId> : CharacterSprite<TSpriteId, SpriteAnimation>
        where TSpriteId : Enum
    {
        public LargeCharacterSprite(TSpriteId id, Vector2 position) : base(id, position) { }
    }
}