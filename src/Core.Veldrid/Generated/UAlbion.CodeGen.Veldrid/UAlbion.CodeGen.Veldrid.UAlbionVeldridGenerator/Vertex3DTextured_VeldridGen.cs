using Veldrid;
namespace UAlbion.Core.Veldrid.Sprites
{
    public partial struct Vertex3DTextured
    {
        public static VertexLayoutDescription GetLayout(bool input) => new(
            new VertexElementDescription((input ? "i" : "o") + "Position", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
            new VertexElementDescription((input ? "i" : "o") + "TexCoords", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2));
    }
}
