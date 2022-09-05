using Veldrid;
namespace UAlbion.Core.Veldrid.Meshes
{
    internal partial struct MeshIntermediate
    {
        public static VertexLayoutDescription GetLayout(bool input) => new(
            new VertexElementDescription((input ? "i" : "o") + "TexCoords", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2));
    }
}
