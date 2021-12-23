using Veldrid;
namespace UAlbion.Core.Veldrid
{
    public partial struct Vertex2DTextured
    {
        public static VertexLayoutDescription GetLayout(bool input) => new(
            new VertexElementDescription((input ? "i" : "o") + "Position", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2),
            new VertexElementDescription((input ? "i" : "o") + "TexCoords", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2));
    }
}
