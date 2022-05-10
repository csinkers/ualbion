using System.Numerics;
using UAlbion.Core.Visual;
using VeldridGen.Interfaces;

namespace UAlbion.Core.Veldrid.Sprites;

// ReSharper disable UnusedMember.Global
#pragma warning disable 649 // CS0649 Field is never assigned to, and will always have its default value
#pragma warning disable CA1051 // Do not declare visible instance fields
internal partial struct GpuSpriteInstanceData : IVertexFormat
{
    [Vertex("Flags", EnumPrefix = "SF")] public SpriteFlags Flags;
    [Vertex("InstancePos")] public Vector4 Position;
    [Vertex("Size")]        public Vector2 Size;
    [Vertex("TexOffset")]   public Vector2 TexPosition; // Normalised texture coordinates
    [Vertex("TexSize")]     public Vector2 TexSize; // Normalised texture coordinates
    [Vertex("TexLayer")]    public uint TexLayer;
}
#pragma warning restore CA1051 // Do not declare visible instance fields
#pragma warning restore 649