using Veldrid;
namespace UAlbion.Core.Veldrid.Meshes
{
    public partial struct GpuMeshInstanceData
    {
        public static VertexLayoutDescription GetLayout(bool input) => new(
            new VertexElementDescription((input ? "i" : "o") + "InstancePos", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
            new VertexElementDescription((input ? "i" : "o") + "InstanceScale", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3));
    }
}
