using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using VeldridGen.Interfaces;

namespace UAlbion.Core.Veldrid.Skybox
{
#pragma warning disable 649
    [SuppressMessage("Microsoft.Naming", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "Used for code generation")]
    partial struct SkyboxIntermediate : IVertexFormat
    {
        [Vertex("TexPosition")] public Vector2 TextureCoordinates;
        [Vertex("NormCoords")] public Vector2 NormalisedSpriteCoordinates;
        [Vertex("WorldPosition")] public Vector3 WorldPosition;
    }
#pragma warning restore 649
}