using VeldridGen.Interfaces;

namespace UAlbion.Core.Veldrid.Skybox;

[VertexShader(typeof(SkyboxVertexShader))]
[FragmentShader(typeof(SkyboxFragmentShader))]
partial class SkyboxPipeline : PipelineHolder
{
}