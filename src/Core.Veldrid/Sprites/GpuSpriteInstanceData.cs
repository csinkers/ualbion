using System.Numerics;
using UAlbion.Core.Visual;
using VeldridGen.Interfaces;

namespace UAlbion.Core.Veldrid.Sprites
{
#pragma warning disable 649 // CS0649 Field is never assigned to, and will always have its default value

    internal partial struct GpuSpriteInstanceData : IVertexFormat
    {
        [Vertex("InstancePos")] public Vector4 Position;
        [Vertex("Size")]        public Vector2 Size;
        [Vertex("TexOffset")]   public Vector2 TexPosition; // Normalised texture coordinates
        [Vertex("TexSize")]     public Vector2 TexSize; // Normalised texture coordinates
        [Vertex("TexLayer")]    public uint TexLayer;
        [Vertex("Flags", EnumPrefix = "SF")] public SpriteFlags Flags;
    }
#pragma warning restore 649
}
