using System;
using System.Numerics;

namespace UAlbion.Core.Objects
{
    public class Sprite : SpriteRenderer.ISprite
    {
        // Set everything using Initialize so we can use object pooling if need be.
        public void Initialize(Vector2 position, ITexture texture, int renderOrder, SpriteFlags flags)
        {
            Texture = texture ?? throw new ArgumentNullException(nameof(texture));
            Position = position;
            RenderOrder = renderOrder;
            Flags = flags;
        }

        // From IRenderable
        public Type Renderer => typeof(SpriteRenderer);
        public int RenderOrder { get; private set; }

        // From ISprite
        public SpriteFlags Flags { get; private set; }
        public ITexture Texture { get; private set; }
        public Vector2 Position { get; private set; }
        public Vector2 Size => new Vector2(Texture.Width, Texture.Height);
        public Vector2 TexPosition { get; } = new Vector2(0.0f, 0.0f);
        public Vector2 TexSize { get; } = new Vector2(1.0f, 1.0f);
    }
}