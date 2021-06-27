using Veldrid;
namespace UAlbion.Core.Veldrid
{
    internal partial struct SkyboxIntermediate
    {
        public static VertexLayoutDescription Layout = new(
            new VertexElementDescription("oTexPosition", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2),
            new VertexElementDescription("oNormCoords", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2),
            new VertexElementDescription("oWorldPosition", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3));
    }
}
