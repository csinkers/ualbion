using Veldrid;
namespace UAlbion.Core.Veldrid.Skybox
{
    internal partial struct SkyboxIntermediate
    {
        public static VertexLayoutDescription GetLayout(bool input) => new(
            new VertexElementDescription((input ? "i" : "o") + "TexPosition", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2),
            new VertexElementDescription((input ? "i" : "o") + "NormCoords", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2),
            new VertexElementDescription((input ? "i" : "o") + "WorldPosition", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3));
    }
}
