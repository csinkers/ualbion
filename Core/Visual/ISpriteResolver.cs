using System;

namespace UAlbion.Core.Visual
{
    public interface ISpriteResolver
    {
        Tuple<SpriteKey, SpriteInstanceData> Resolve(SpriteDefinition spriteDefinition);
    }
}