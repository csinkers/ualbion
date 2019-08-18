using System;
using System.Numerics;

namespace UAlbion.Core.Visual
{
    public abstract class SpriteDefinition : IRenderable
    {
        protected SpriteDefinition(int subObject, Vector3 position, int renderOrder, SpriteFlags flags, Vector2? size)
        {
            SubObject = subObject;
            Position = position;
            RenderOrder = renderOrder;
            Flags = flags;
            Size = size;
        }

        public Type Renderer => typeof(SpriteRenderer);
        public int RenderOrder { get; }
        public SpriteFlags Flags { get; }
        public int SubObject { get; }
        public Vector3 Position { get; }
        public Vector2? Size { get; }
        public abstract Type IdType { get; }
        public abstract int NumericId { get; }
    }

    public class SpriteDefinition<T> : SpriteDefinition where T : Enum
    {
        public SpriteDefinition(T id, int subObject, Vector3 position, int renderOrder, SpriteFlags flags, Vector2? size = null)
            : base(subObject, position, renderOrder, flags, size) { Id = id; }

        public T Id { get; }
        public override Type IdType => typeof(T);
        public override int NumericId => (int)(object)Id;
    }
}