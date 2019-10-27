using System;
using System.Numerics;
using Veldrid.Utilities;

namespace UAlbion.Core.Visual
{
    public abstract class SpriteDefinition : IRenderable
    {
        Vector3 _position;

        protected SpriteDefinition(string name, int subObject, Vector3 position, int renderOrder, SpriteFlags flags, Vector2? size)
        {
            Name = name;
            SubObject = subObject;
            Position = position;
            RenderOrder = renderOrder;
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
        public Matrix4x4 Transform { get; private set; } = Matrix4x4.Identity;
        public event EventHandler ExtentsChanged;
        public int RenderOrder { get; }
        public SpriteFlags Flags { get; set; }
        public int SubObject { get; }
        
        public Vector3 Position
        {
            get => _position;
            set
            {
                _position = value;
                Transform = Matrix4x4.CreateTranslation(_position);
                ExtentsChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public Vector2? Size { get; set; }
        public abstract Type IdType { get; }
        public abstract int NumericId { get; }
    }

    public class SpriteDefinition<T> : SpriteDefinition where T : Enum
    {
        public SpriteDefinition(T id, int subObject, Vector3 position, int renderOrder, SpriteFlags flags, Vector2? size = null)
            : base(id.ToString(), subObject, position, renderOrder, flags, size) { Id = id; }

        public T Id { get; }
        public override Type IdType => typeof(T);
        public override int NumericId => (int)(object)Id;
    }
}