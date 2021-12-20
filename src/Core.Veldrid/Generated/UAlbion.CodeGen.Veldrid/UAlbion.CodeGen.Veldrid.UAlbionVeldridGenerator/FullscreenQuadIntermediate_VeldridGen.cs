using Veldrid;
namespace UAlbion.Core.Veldrid
{
    internal partial struct FullscreenQuadIntermediate
    {
        public static VertexLayoutDescription GetLayout(bool input) => new(
            new VertexElementDescription((input ? "i" : "o") + "NormCoords", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2));
    }
}
