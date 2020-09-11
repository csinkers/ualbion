using Veldrid;

namespace UAlbion.Core.Veldrid
{
    public static class VertexLayoutHelper
    {
        public static VertexElementDescription IntElement(string name) => new VertexElementDescription(name, VertexElementSemantic.TextureCoordinate, VertexElementFormat.Int1);
        public static VertexElementDescription UIntElement(string name) => new VertexElementDescription(name, VertexElementSemantic.TextureCoordinate, VertexElementFormat.UInt1);
        public static VertexElementDescription FloatElement(string name) => new VertexElementDescription(name, VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float1);
        public static VertexElementDescription Vector2D(string name) => new VertexElementDescription(name, VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2);
        public static VertexElementDescription Vector3D(string name) => new VertexElementDescription(name, VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3);
        public static VertexElementDescription Vector4D(string name) => new VertexElementDescription(name, VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float4);

        public static VertexLayoutDescription Vertex2DTextured => new VertexLayoutDescription(
            Vector2D("Position"),
            Vector2D("TexCoords"));

        public static VertexLayoutDescription Vertex3DTextured => new VertexLayoutDescription(
            Vector3D("Position"),
            Vector2D("TexCoords"));
    }
}
