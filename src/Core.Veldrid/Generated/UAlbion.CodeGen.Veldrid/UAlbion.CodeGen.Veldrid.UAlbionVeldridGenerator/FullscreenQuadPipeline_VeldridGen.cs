using Veldrid;
namespace UAlbion.Core.Veldrid
{
    internal partial class FullscreenQuadPipeline
    {

        public FullscreenQuadPipeline() : base("FullscreenQuadSV.vert", "FullscreenQuadSF.frag",
            new[] { global::UAlbion.Core.Veldrid.Sprites.Vertex2DTextured.GetLayout(true)},
            new[] { typeof(global::UAlbion.Core.Veldrid.FullscreenQuadResourceSet) })
        { }
    }
}
