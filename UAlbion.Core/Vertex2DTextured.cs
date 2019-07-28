using System.Numerics;
using Veldrid;

namespace UAlbion.Core
{
    public static class ResourceLayoutH
    {
        public static ResourceLayoutElementDescription Texture(string name) => new ResourceLayoutElementDescription(name, ResourceKind.TextureReadOnly, ShaderStages.Fragment);
        public static ResourceLayoutElementDescription Sampler(string name) => new ResourceLayoutElementDescription(name, ResourceKind.Sampler, ShaderStages.Fragment);
    }

    public static class VertexLayoutH
    {
        public static VertexElementDescription Position2D(string name) => new VertexElementDescription(name, VertexElementSemantic.Position, VertexElementFormat.Float2);
        public static VertexElementDescription Position3D(string name) => new VertexElementDescription(name, VertexElementSemantic.Position, VertexElementFormat.Float3);
        public static VertexElementDescription Texture2D(string name) => new VertexElementDescription(name, VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2);
        public static VertexElementDescription Color(string name) => new VertexElementDescription(name, VertexElementSemantic.Color, VertexElementFormat.Float3);
        public static VertexElementDescription Normal(string name) => new VertexElementDescription(name, VertexElementSemantic.Normal, VertexElementFormat.Float3);
    }

    internal struct Vertex2DTextured
    {
        public float X;
        public float Y;
        public float U;
        public float V;

        public Vertex2DTextured(Vector2 position, Vector2 textureCoordinates)
        {
            X = position.X;
            Y = position.Y;
            U = textureCoordinates.X;
            V = textureCoordinates.Y;
        }

        public Vertex2DTextured(float x, float y, float u, float v) { X = x; Y = y; U = u; V = v; }

        public static VertexLayoutDescription VertexLayout => new VertexLayoutDescription(
            VertexLayoutH.Position2D("Position"),
            VertexLayoutH.Texture2D("TexCoords"));
    }
}