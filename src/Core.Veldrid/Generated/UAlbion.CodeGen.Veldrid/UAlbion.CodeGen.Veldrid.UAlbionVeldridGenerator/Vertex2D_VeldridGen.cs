using Veldrid;
namespace UAlbion.Core.Veldrid
{
    public partial struct Vertex2D
    {
        public static VertexLayoutDescription GetLayout(bool input) => new(
            new VertexElementDescription((input ? "i" : "o") + "Position", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2));
    }
}
