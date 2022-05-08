using Veldrid;
namespace UAlbion.Core.Veldrid.Sprites
{
    internal partial class BlendedSpritePipeline
    {
        static VertexLayoutDescription GpuBlendedSpriteInstanceDataLayout
        {
            get
            {
                var layout = global::UAlbion.Core.Veldrid.Sprites.GpuBlendedSpriteInstanceData.GetLayout(true);
                layout.InstanceStepRate = 1;
                return layout;
            }
        }


        public BlendedSpritePipeline() : base("BlendedSpriteSV.vert", "BlendedSpriteSF.frag",
            new[] { global::UAlbion.Core.Veldrid.Vertex2DTextured.GetLayout(true), GpuBlendedSpriteInstanceDataLayout},
            new[] { typeof(global::UAlbion.Core.Veldrid.CommonSet), typeof(global::UAlbion.Core.Veldrid.Sprites.SpriteSet) })
        { }
    }
}
