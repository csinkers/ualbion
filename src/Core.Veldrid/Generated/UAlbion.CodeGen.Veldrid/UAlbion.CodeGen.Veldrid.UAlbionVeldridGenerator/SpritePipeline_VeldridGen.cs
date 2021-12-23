using Veldrid;
namespace UAlbion.Core.Veldrid.Sprites
{
    internal partial class SpritePipeline
    {
        static VertexLayoutDescription GpuSpriteInstanceDataLayout
        {
            get
            {
                var layout = global::UAlbion.Core.Veldrid.Sprites.GpuSpriteInstanceData.GetLayout(true);
                layout.InstanceStepRate = 1;
                return layout;
            }
        }


        public SpritePipeline() : base("SpriteSV.vert", "SpriteSF.frag",
            new[] { global::UAlbion.Core.Veldrid.Vertex2DTextured.GetLayout(true), GpuSpriteInstanceDataLayout},
            new[] { typeof(global::UAlbion.Core.Veldrid.CommonSet), typeof(global::UAlbion.Core.Veldrid.Sprites.SpriteSet) })
        { }
    }
}
