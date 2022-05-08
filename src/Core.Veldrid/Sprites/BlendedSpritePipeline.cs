using VeldridGen.Interfaces;

namespace UAlbion.Core.Veldrid.Sprites;

[VertexShader(typeof(BlendedSpriteVertexShader))]
[FragmentShader(typeof(BlendedSpriteFragmentShader))]
internal partial class BlendedSpritePipeline : PipelineHolder { }