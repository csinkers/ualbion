using Veldrid;
namespace UAlbion.Core.Veldrid
{
    internal partial class SkyboxPipeline
    {

        public SkyboxPipeline() : base("SkyBoxSV.vert", "SkyBoxSF.frag",
            new[] { global::UAlbion.Core.Veldrid.Sprites.Vertex2DTextured.GetLayout(true)},
            new[] { typeof(global::UAlbion.Core.Veldrid.SkyboxResourceSet), typeof(global::UAlbion.Core.Veldrid.CommonSet) })
        { }
    }
}
