using Veldrid;
namespace UAlbion.Core.Veldrid
{
    internal partial struct SkyboxIntermediate
    {
        public static VertexLayoutDescription Layout { get; } = new(
            new VertexElementDescription("TexPosition", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2),
            new VertexElementDescription("NormCoords", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2),
            new VertexElementDescription("WorldPosition", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3));
    }
}
