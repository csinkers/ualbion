using Veldrid;
namespace UAlbion.Core.Veldrid.Etm
{
    internal partial class EtmPipeline
    {
        static VertexLayoutDescription DungeonTileLayout
        {
            get
            {
                var layout = global::UAlbion.Core.Veldrid.Etm.DungeonTile.GetLayout(true);
                layout.InstanceStepRate = 1;
                return layout;
            }
        }

        public EtmPipeline() : base("ExtrudedTileMapSV.vert", "ExtrudedTileMapSF.frag",
            new[] {
                global::UAlbion.Core.Veldrid.Vertex3DTextured.GetLayout(true), 
                DungeonTileLayout
            },
            new[] {
                typeof(global::UAlbion.Core.Veldrid.GlobalSet), 
                typeof(global::UAlbion.Core.Veldrid.MainPassSet), 
                typeof(global::UAlbion.Core.Veldrid.Etm.EtmSet)
            })
        { }
    }
}
