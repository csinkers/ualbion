using Veldrid;

namespace UAlbion.Core
{
    public static class VertexLayoutHelper
    {
        public static VertexElementDescription Float(string name) => new VertexElementDescription(name, VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float1);
        public static VertexElementDescription Vector2D(string name) => new VertexElementDescription(name, VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2);
        public static VertexElementDescription Vector3D(string name) => new VertexElementDescription(name, VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3);
        public static VertexElementDescription Int(string name) => new VertexElementDescription(name, VertexElementSemantic.TextureCoordinate, VertexElementFormat.Int1);
    }
}