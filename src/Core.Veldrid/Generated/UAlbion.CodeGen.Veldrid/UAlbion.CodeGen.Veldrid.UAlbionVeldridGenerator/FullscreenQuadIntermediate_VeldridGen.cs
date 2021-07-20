using Veldrid;
namespace UAlbion.Core.Veldrid
{
    internal partial struct FullscreenQuadIntermediate
    {
        public static VertexLayoutDescription Layout { get; } = new(
            new VertexElementDescription("NormCoords", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2));
    }
}
