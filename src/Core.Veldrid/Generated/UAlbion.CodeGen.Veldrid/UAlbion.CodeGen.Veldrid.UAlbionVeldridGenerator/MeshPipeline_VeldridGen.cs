using Veldrid;
namespace UAlbion.Core.Veldrid.Meshes
{
    internal partial class MeshPipeline
    {
        static VertexLayoutDescription GpuMeshInstanceDataLayout
        {
            get
            {
                var layout = global::UAlbion.Core.Veldrid.Meshes.GpuMeshInstanceData.GetLayout(true);
                layout.InstanceStepRate = 1;
                return layout;
            }
        }

        public MeshPipeline() : base("MeshSV.vert", "MeshSF.frag",
            new[] {
                global::UAlbion.Core.Veldrid.Meshes.MeshVertex.GetLayout(true), 
                GpuMeshInstanceDataLayout
            },
            new[] {
                typeof(global::UAlbion.Core.Veldrid.GlobalSet), 
                typeof(global::UAlbion.Core.Veldrid.MainPassSet), 
                typeof(global::UAlbion.Core.Veldrid.Meshes.MeshResourceSet)
            })
        { }
    }
}
