using System;
using System.Numerics;
using Veldrid.Utilities;

namespace UAlbion.Core.Visual
{
    public abstract class SpriteDefinition : IRenderable
    {
        protected SpriteDefinition(string name, int subObject, Vector3 position, int renderOrder, bool depthTested, SpriteFlags flags, Vector2? size)
        {
            Name = name;
            SubObject = subObject;
            Position = position;
            RenderOrder = renderOrder;
            DepthTested = depthTested;
            Flags = flags;
            Size = size;
        }

        public string Name { get; }
        public Type Renderer => typeof(SpriteRenderer);

        public BoundingBox? Extents
        {
            get
            {
                var min = Position;
                var max = Position + new Vector3(Size?.X ?? 1, Size?.Y ?? 1, Size?.X ?? 1);
                return new BoundingBox(min, max);
            }
        }
        public event EventHandler ExtentsChanged;
        public bool DepthTested { get; }
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
        public SpriteDefinition(T id, int subObject, Vector3 position, int renderOrder, bool depthTested, SpriteFlags flags, Vector2? size = null)
            : base(id.ToString(), subObject, position, renderOrder, depthTested, flags, size) { Id = id; }

        public T Id { get; }
        public override Type IdType => typeof(T);
        public override int NumericId => (int)(object)Id;
    }
}