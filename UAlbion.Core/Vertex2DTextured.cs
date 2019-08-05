using System.Numerics;
using Veldrid;

namespace UAlbion.Core
{
    public static class ResourceLayoutH
    {
        public static ResourceLayoutElementDescription Uniform(string name) => new ResourceLayoutElementDescription(name, ResourceKind.UniformBuffer, ShaderStages.Vertex);
        public static ResourceLayoutElementDescription Texture(string name) => new ResourceLayoutElementDescription(name, ResourceKind.TextureReadOnly, ShaderStages.Fragment);
        public static ResourceLayoutElementDescription Sampler(string name) => new ResourceLayoutElementDescription(name, ResourceKind.Sampler, ShaderStages.Fragment);
    }

    public static class VertexLayoutH
    {
        public static VertexElementDescription Vector2D(string name) => new VertexElementDescription(name, VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2);
        public static VertexElementDescription Vector3D(string name) => new VertexElementDescription(name, VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3);
        public static VertexElementDescription Int(string name) => new VertexElementDescription(name, VertexElementSemantic.TextureCoordinate, VertexElementFormat.Int1);
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
            VertexLayoutH.Vector2D("Position"),
            VertexLayoutH.Vector2D("TexCoords"));
    }
}