using System;
using System.Numerics;

namespace UAlbion.Core.Objects
{
    public abstract class SpriteDefinition : IRenderable
    {
        protected SpriteDefinition(int subObject, Vector2 position, int renderOrder, SpriteFlags flags)
        {
            SubObject = subObject;
            Position = position;
            RenderOrder = renderOrder;
            Flags = flags;
        }

        public Type Renderer => typeof(SpriteRenderer);
        public int RenderOrder { get; }
        public SpriteFlags Flags { get; }
        public int SubObject { get; }
        public Vector2 Position { get; }
        public abstract Type IdType { get; }
        public abstract int NumericId { get; }
    }

    public class SpriteDefinition<T> : SpriteDefinition where T : Enum
    {
        public SpriteDefinition(T id, int subObject, Vector2 position, int renderOrder, SpriteFlags flags)
            : base(subObject, position, renderOrder, flags) { Id = id; }

        public T Id { get; }
        public override Type IdType => typeof(T);
        public override int NumericId => (int)(object)Id;
    }
}