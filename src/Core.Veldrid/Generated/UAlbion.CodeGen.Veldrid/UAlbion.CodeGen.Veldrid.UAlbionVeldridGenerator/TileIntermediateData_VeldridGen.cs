using Veldrid;
namespace UAlbion.Core.Veldrid.Sprites
{
    internal partial struct TileIntermediateData
    {
        public static VertexLayoutDescription GetLayout(bool input) => new(
            new VertexElementDescription((input ? "i" : "o") + "WorldPosition", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float4),
            new VertexElementDescription((input ? "i" : "o") + "TilePosition", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2));
    }
}
