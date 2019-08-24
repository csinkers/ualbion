using System;
using System.Collections.Generic;
using System.Linq;

namespace UAlbion.Core.Visual
{
    public class MultiSprite : IRenderable
    {
        public MultiSprite(SpriteKey key)
        {
            Key = key;
        }

        public MultiSprite(SpriteKey key, int bufferId, IEnumerable<SpriteInstanceData> sprites)
        {
            Key = key;
            BufferId = bufferId;

            if (sprites is SpriteInstanceData[] array)
                Instances = array;
            else
                Instances = sprites.ToArray();
        }

        public string Name => Key.Texture.Name;
        public int RenderOrder => Key.RenderOrder;
        public Type Renderer => typeof(SpriteRenderer);
        public SpriteKey Key { get; }
        public int BufferId { get; set; }
        public SpriteInstanceData[] Instances { get; set; }
    }
}