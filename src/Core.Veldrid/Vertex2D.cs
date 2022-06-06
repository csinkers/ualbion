using System.Numerics;
using VeldridGen.Interfaces;

namespace UAlbion.Core.Veldrid;

#pragma warning disable CA1815 // Override equals and operator equals on value types
public readonly partial struct Vertex2D : IVertexFormat
{
    [Vertex("Position")] public Vector2 Position { get; }
    public Vertex2D(float x, float y) => Position = new Vector2(x, y);
}
#pragma warning restore CA1815 // Override equals and operator equals on value types