using Veldrid;
namespace UAlbion.Core.Veldrid.Meshes
{
    public partial struct MeshInstanceData
    {
        public static VertexLayoutDescription GetLayout(bool input) => new(
            new VertexElementDescription((input ? "i" : "o") + "InstancePos", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float4));
    }
}
