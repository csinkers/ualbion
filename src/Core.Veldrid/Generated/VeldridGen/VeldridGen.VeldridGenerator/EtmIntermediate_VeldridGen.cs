using Veldrid;
namespace UAlbion.Core.Veldrid
{
    internal partial class EtmIntermediate
    {
        public static VertexLayoutDescription Layout = new(
            new VertexElementDescription("TexCoords", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2),
            new VertexElementDescription("Textures", VertexElementSemantic.TextureCoordinate, VertexElementFormat.UInt1),
            new VertexElementDescription("Flags", VertexElementSemantic.TextureCoordinate, VertexElementFormat.UInt1));
    }
}
