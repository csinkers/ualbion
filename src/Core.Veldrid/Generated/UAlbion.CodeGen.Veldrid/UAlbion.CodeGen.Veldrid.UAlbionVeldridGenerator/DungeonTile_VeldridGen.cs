using Veldrid;
namespace UAlbion.Core.Veldrid.Etm
{
    public partial struct DungeonTile
    {
        public static VertexLayoutDescription Layout { get; } = new(
            new VertexElementDescription("Textures", VertexElementSemantic.TextureCoordinate, VertexElementFormat.UInt1),
            new VertexElementDescription("WallSize", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2),
            new VertexElementDescription("Flags", VertexElementSemantic.TextureCoordinate, VertexElementFormat.UInt1));
    }
}
