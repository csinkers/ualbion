using System.Numerics;
using System.Runtime.CompilerServices;
using Veldrid;

namespace UAlbion.Core.Visual
{
    public struct SpriteInstanceData
    {
        public static readonly uint StructSize = (uint)Unsafe.SizeOf<SpriteInstanceData>();
        public static readonly VertexLayoutDescription VertexLayout = new VertexLayoutDescription(
                VertexLayoutHelper.Vector3D("Offset"), VertexLayoutHelper.Vector2D("Size"),
                VertexLayoutHelper.Vector2D("TexPosition"), VertexLayoutHelper.Vector2D("TexSize"),
                VertexLayoutHelper.Int("TexLayer"), VertexLayoutHelper.Int("Flags")
            )
            { InstanceStepRate = 1 };
        public SpriteInstanceData(Vector3 position, Vector2 size, Vector2 texPosition, Vector2 texSize, int texLayer, SpriteFlags flags)
        {
            Offset = position; Size = size;
            TexPosition = texPosition; TexSize = texSize;
            TexLayer = texLayer;
            Flags = flags;
        }

        public Vector3 Offset { get; set; } // Pixel coordinates
        public Vector2 Size { get; set; } // Pixel coordinates
        public Vector2 TexPosition { get; set; } // Normalised texture coordinates
        public Vector2 TexSize { get; set; } // Normalised texture coordinates
        public int TexLayer { get; set; }
        public SpriteFlags Flags { get; set; }
    }
}