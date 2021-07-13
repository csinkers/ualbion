using Veldrid;

namespace UAlbion.Core.Veldrid
{
    public static class VertexLayoutHelper
    {
        public static VertexElementDescription IntElement(string name) => new(name, VertexElementSemantic.TextureCoordinate, VertexElementFormat.Int1);
        public static VertexElementDescription UIntElement(string name) => new(name, VertexElementSemantic.TextureCoordinate, VertexElementFormat.UInt1);
        public static VertexElementDescription FloatElement(string name) => new(name, VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float1);
        public static VertexElementDescription Vector2D(string name) => new(name, VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2);
        public static VertexElementDescription Vector3D(string name) => new(name, VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3);
        public static VertexElementDescription Vector4D(string name) => new(name, VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float4);

        public static VertexLayoutDescription Vertex2DTextured => new(
            Vector2D("vPosition"),
            Vector2D("vTexCoords"));

        public static VertexLayoutDescription Vertex3DTextured => new(
            Vector3D("vPosition"),
            Vector2D("vTexCoords"));
    }
}
