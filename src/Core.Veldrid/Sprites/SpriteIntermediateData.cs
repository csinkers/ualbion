using System.Numerics;
using UAlbion.Core.Visual;
using VeldridGen.Interfaces;

namespace UAlbion.Core.Veldrid.Sprites;
#pragma warning disable 649 // CS0649 Field is never assigned to, and will always have its default value
internal partial struct SpriteIntermediateData : IVertexFormat
{
    [Vertex("TexPosition")] public Vector2 TexturePosition;
    [Vertex("Layer", Flat = true)] public float TextureLayer;
    [Vertex("UvClamp")] public Vector4 UvClamp;
    [Vertex("Flags", Flat = true, EnumPrefix = "SF")] public SpriteFlags Flags;
    [Vertex("NormCoords")] public Vector2 NormalisedSpriteCoordinates;
    [Vertex("WorldPosition")] public Vector3 WorldPosition;
}
#pragma warning restore 649