using System;
using System.Numerics;

namespace UAlbion.Core.Visual
{
    public interface ISpriteResolver
    {
        Tuple<SpriteKey, SpriteInstanceData> Resolve(SpriteDefinition spriteDefinition);
        Vector2 GetSize(Type idType, int id, int subObject);
    }
}