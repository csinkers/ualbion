using System;
using System.Numerics;

namespace UAlbion.Core.Objects
{
    /*
    public class Sprite : SpriteRenderer.ISprite
    {
        // Set everything using Initialize so we can use object pooling if need be.
        public void Initialize(Vector2 position, ITexture texture, int renderOrder, SpriteFlags flags)
        {
            Key = new SpriteRenderer.SpriteKey(texture, renderOrder);
            Position = position;
            Flags = flags;
        }

        // From IRenderable
        public Type Renderer => typeof(SpriteRenderer);
        public int RenderOrder => Key.RenderOrder;

        // From ISprite
        public SpriteRenderer.SpriteKey Key { get; private set; }
        public Vector2 Position { get; private set; }
        public Vector2 Size => new Vector2(Key.Texture.Width, Key.Texture.Height);
        public Vector2 TexPosition { get; } = new Vector2(0.0f, 0.0f);
        public Vector2 TexSize { get; } = new Vector2(1.0f, 1.0f);
        public SpriteFlags Flags { get; private set; }
    }
    */
}