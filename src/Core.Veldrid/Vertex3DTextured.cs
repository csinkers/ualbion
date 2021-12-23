using System.Numerics;
using VeldridGen.Interfaces;

namespace UAlbion.Core.Veldrid
{
#pragma warning disable CA1815 // Override equals and operator equals on value types
    public readonly partial struct Vertex3DTextured : IVertexFormat
    {
        [Vertex("Position")] public Vector3 Position { get; }
        [Vertex("TexCoords")] public Vector2 TextureCoordinates { get; }
        public float X => Position.X;
        public float Y => Position.Y;
        public float Z => Position.Z;
        public float U => TextureCoordinates.X;
        public float V => TextureCoordinates.Y;

        public Vertex3DTextured(Vector3 position, Vector2 textureCoordinates)
        {
            Position = position;
            TextureCoordinates = textureCoordinates;
        }

        public Vertex3DTextured(float x, float y, float z, float u, float v)
        {
            Position = new Vector3(x, y, z);
            TextureCoordinates = new Vector2(u, v);
        }
    }
#pragma warning restore CA1815 // Override equals and operator equals on value types
}