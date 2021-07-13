using Veldrid;
namespace UAlbion.Core.Veldrid.Sprites
{
    public partial struct Vertex3DTextured
    {
        public static VertexLayoutDescription Layout { get; } = new(
            new VertexElementDescription("Position", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
            new VertexElementDescription("TexCoords", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2));
    }
}
