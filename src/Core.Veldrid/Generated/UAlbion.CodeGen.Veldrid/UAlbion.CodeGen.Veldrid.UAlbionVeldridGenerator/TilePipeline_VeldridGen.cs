using Veldrid;
namespace UAlbion.Core.Veldrid.Sprites
{
    internal partial class TilePipeline
    {

        public TilePipeline() : base("TilesSV.vert", "TilesSF.frag",
            new[] { global::UAlbion.Core.Veldrid.Vertex2D.GetLayout(true)},
            new[] { typeof(global::UAlbion.Core.Veldrid.CommonSet), typeof(global::UAlbion.Core.Veldrid.Sprites.TilesetResourceSet), typeof(global::UAlbion.Core.Veldrid.Sprites.TileLayerResourceSet) })
        { }
    }
}
