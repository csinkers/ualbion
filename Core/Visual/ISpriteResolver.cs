using System;

namespace UAlbion.Core.Visual
{
    public interface ISpriteResolver
    {
        Tuple<SpriteRenderer.SpriteKey, SpriteRenderer.InstanceData> Resolve(SpriteDefinition spriteDefinition);
    }
}