using Veldrid;
namespace UAlbion.Core.Veldrid.Sprites
{
    public partial struct ColorOnly
    {
        public static VertexLayoutDescription Layout = new(
            new VertexElementDescription("Color", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float4));
    }
}
