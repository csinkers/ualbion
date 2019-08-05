using System;

namespace UAlbion.Core.Objects
{
    public interface ISpriteResolver
    {
        Tuple<SpriteRenderer.SpriteKey, SpriteRenderer.InstanceData> Resolve(SpriteDefinition spriteDefinition);
    }
}