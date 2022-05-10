using System.Numerics;
using UAlbion.Core.Visual;
using VeldridGen.Interfaces;
// ReSharper disable UnusedMember.Global

namespace UAlbion.Core.Veldrid.Sprites;

#pragma warning disable 649 // CS0649 Field is never assigned to, and will always have its default value
#pragma warning disable CA1051 // Do not declare visible instance fields
internal partial struct GpuBlendedSpriteInstanceData : IVertexFormat
{
    [Vertex("Flags", EnumPrefix = "SF")] public SpriteFlags Flags;
    [Vertex("InstancePos")] public Vector4 Position;
    [Vertex("Size")]        public Vector2 Size;
    [Vertex("TexOffset1")]  public Vector2 TexPosition1; // Normalised texture coordinates
    [Vertex("TexSize1")]    public Vector2 TexSize1; // Normalised texture coordinates
    [Vertex("TexLayer1")]   public uint TexLayer1;
    [Vertex("TexOffset2")]  public Vector2 TexPosition2; // Normalised texture coordinates
    [Vertex("TexSize2")]    public Vector2 TexSize2; // Normalised texture coordinates
    [Vertex("TexLayer2")]   public uint TexLayer2;
}
#pragma warning restore CA1051 // Do not declare visible instance fields
#pragma warning restore 649