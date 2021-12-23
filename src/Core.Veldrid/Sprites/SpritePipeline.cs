using VeldridGen.Interfaces;

namespace UAlbion.Core.Veldrid.Sprites
{
    [VertexShader(typeof(SpriteVertexShader))]
    [FragmentShader(typeof(SpriteFragmentShader))]
    internal partial class SpritePipeline : PipelineHolder { }
}