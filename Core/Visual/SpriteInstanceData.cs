using System.Numerics;
using System.Runtime.CompilerServices;

namespace UAlbion.Core.Visual
{
    public struct SpriteInstanceData
    {
        public static readonly uint StructSize = (uint)Unsafe.SizeOf<SpriteInstanceData>();
        public SpriteInstanceData(Vector3 position, Vector2 size, Vector2 texPosition, Vector2 texSize, uint texLayer, SpriteFlags flags)
        {
            Offset = position; Size = size;
            TexPosition = texPosition; TexSize = texSize;
            TexLayer = texLayer;
            Flags = flags;
            Rotation = 0;
        }

        public Vector3 Offset { get; set; } // Pixel coordinates
        public Vector2 Size { get; set; } // Pixel coordinates
        public Vector2 TexPosition { get; set; } // Normalised texture coordinates
        public Vector2 TexSize { get; set; } // Normalised texture coordinates
        public uint TexLayer { get; set; }
        public SpriteFlags Flags { get; set; }
        public float Rotation { get; set; }
    }
}