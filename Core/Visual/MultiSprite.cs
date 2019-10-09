using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Veldrid.Utilities;

namespace UAlbion.Core.Visual
{
    public class UiMultiSprite : MultiSprite, IScreenSpaceRenderable {
        public UiMultiSprite(SpriteKey key) : base(key) { }
        public UiMultiSprite(SpriteKey key, int bufferId, IEnumerable<SpriteInstanceData> sprites) : base(key, bufferId, sprites) { }
    }

    public class MultiSprite : IRenderable
    {
        public MultiSprite(SpriteKey key) { Key = key; }
        public override string ToString() => $"Multi:{Name} {RenderOrder} Depth:{DepthTested} ({Instances.Length} instances)";

        public MultiSprite(SpriteKey key, int bufferId, IEnumerable<SpriteInstanceData> sprites)
        {
            Key = key;
            BufferId = bufferId;

            if (sprites is SpriteInstanceData[] array)
                Instances = array;
            else
                Instances = sprites.ToArray();
            CalculateExtents();
        }

        public void CalculateExtents()
        {
            Vector3 min = Vector3.Zero;
            Vector3 max = Vector3.Zero;
            bool first = true;
            foreach(var instance in Instances)
            {
                if (first)
                {
                    min = instance.Offset;
                    max = instance.Offset + new Vector3(instance.Size.X, instance.Size.Y, instance.Size.X);
                }
                else
                {
                    min = Vector3.Min(min, instance.Offset);
                    max = Vector3.Max(max, instance.Offset + new Vector3(instance.Size.X, instance.Size.Y, instance.Size.X));
                }

                first = false;
            }
            _extents = new BoundingBox(min, max);
            ExtentsChanged?.Invoke(this, EventArgs.Empty);
        }

        public void RotateSprites(Vector3 cameraPosition)
        {
            for(int i = 0; i < Instances.Length; i++)
            {
                if ((Instances[i].Flags & SpriteFlags.Billboard) == 0)
                    continue;

                var delta = Instances[i].Offset - cameraPosition;
                Instances[i].Rotation = -(float)Math.Atan2(delta.X, delta.Z);
            }
        }

        BoundingBox _extents;
        Vector3 _position;

        public string Name => Key.Texture.Name;
        public int RenderOrder => Key.RenderOrder;
        public Type Renderer => typeof(SpriteRenderer);

        public BoundingBox? Extents => new BoundingBox(_extents.Min + Position, _extents.Max + Position);
        public Matrix4x4 Transform { get; private set; } = Matrix4x4.Identity;

        public event EventHandler ExtentsChanged;
        public bool DepthTested => Key.DepthTested;
        public SpriteKey Key { get; }
        public int BufferId { get; set; }

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
        public SpriteInstanceData[] Instances { get; set; }
    }
}