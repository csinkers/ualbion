using Veldrid;
namespace UAlbion.Core.Veldrid.Sprites
{
    public partial class SpritePipeline
    {
        static VertexLayoutDescription GpuSpriteInstanceDataLayout
        {
            get
            {
                var layout = global::UAlbion.Core.Veldrid.Sprites.GpuSpriteInstanceData.Layout;
                layout.InstanceStepRate = 1;
                return layout;
            }
        }


        public SpritePipeline() : base("SpriteSV.vert", "SpriteSF.frag",
            new[] { global::UAlbion.Core.Veldrid.Sprites.Vertex2DTextured.Layout, GpuSpriteInstanceDataLayout},
            new[] { typeof(global::UAlbion.Core.Veldrid.CommonSet), typeof(global::UAlbion.Core.Veldrid.Sprites.SpriteArraySet) })
        { }
    }
}
