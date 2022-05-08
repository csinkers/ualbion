using System.Numerics;
using UAlbion.Core.Visual;
using VeldridGen.Interfaces;

namespace UAlbion.Core.Veldrid.Sprites;

#pragma warning disable 649 // CS0649 Field is never assigned to, and will always have its default value
internal partial struct BlendedSpriteIntermediateData : IVertexFormat
{
    [Vertex("Flags", Flat = true, EnumPrefix = "SF")] public SpriteFlags Flags;
    [Vertex("TexPosition1")] public Vector2 TexturePosition1;
    [Vertex("Layer1", Flat = true)] public float TextureLayer1;
    [Vertex("TexPosition2")] public Vector2 TexturePosition2;
    [Vertex("Layer2", Flat = true)] public float TextureLayer2;
    [Vertex("NormCoords")] public Vector2 NormalisedSpriteCoordinates;
    [Vertex("WorldPosition")] public Vector3 WorldPosition;
}
#pragma warning restore 649