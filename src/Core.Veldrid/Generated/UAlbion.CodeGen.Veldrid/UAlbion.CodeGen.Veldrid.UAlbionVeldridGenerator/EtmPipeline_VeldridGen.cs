using Veldrid;
namespace UAlbion.Core.Veldrid.Etm
{
    internal partial class EtmPipeline
    {
        static VertexLayoutDescription DungeonTileLayout
        {
            get
            {
                var layout = global::UAlbion.Core.Veldrid.Etm.DungeonTile.Layout;
                layout.InstanceStepRate = 1;
                return layout;
            }
        }


        public EtmPipeline() : base("ExtrudedTileMapSV.vert", "ExtrudedTileMapSF.frag",
            new[] { global::UAlbion.Core.Veldrid.Sprites.Vertex3DTextured.Layout, DungeonTileLayout},
            new[] { typeof(global::UAlbion.Core.Veldrid.Etm.EtmSet), typeof(global::UAlbion.Core.Veldrid.CommonSet) })
        { }
    }
}
