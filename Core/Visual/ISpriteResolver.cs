using System;
using System.Numerics;

namespace UAlbion.Core.Visual
{
    public interface ISpriteResolver
    {
        Tuple<SpriteKey, SpriteInstanceData> Resolve(Sprite sprite);
        Vector2 GetSize(Type idType, int id, int subObject);
    }
}