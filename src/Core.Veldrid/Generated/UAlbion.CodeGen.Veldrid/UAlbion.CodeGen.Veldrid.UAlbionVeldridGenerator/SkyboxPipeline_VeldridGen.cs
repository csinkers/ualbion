using Veldrid;
namespace UAlbion.Core.Veldrid.Skybox
{
    internal partial class SkyboxPipeline
    {

        public SkyboxPipeline() : base("SkyBoxSV.vert", "SkyBoxSF.frag",
            new[] { global::UAlbion.Core.Veldrid.Vertex2DTextured.GetLayout(true)},
            new[] { typeof(global::UAlbion.Core.Veldrid.Skybox.SkyboxResourceSet), typeof(global::UAlbion.Core.Veldrid.CommonSet) })
        { }
    }
}
