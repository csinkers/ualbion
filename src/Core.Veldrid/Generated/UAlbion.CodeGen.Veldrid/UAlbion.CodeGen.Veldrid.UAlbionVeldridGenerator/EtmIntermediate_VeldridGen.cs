using Veldrid;
namespace UAlbion.Core.Veldrid.Etm
{
    internal partial class EtmIntermediate
    {
        public static VertexLayoutDescription GetLayout(bool input) => new(
            new VertexElementDescription((input ? "i" : "o") + "TexCoords", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2),
            new VertexElementDescription((input ? "i" : "o") + "Textures", VertexElementSemantic.TextureCoordinate, VertexElementFormat.UInt1),
            new VertexElementDescription((input ? "i" : "o") + "Flags", VertexElementSemantic.TextureCoordinate, VertexElementFormat.UInt1));
    }
}
