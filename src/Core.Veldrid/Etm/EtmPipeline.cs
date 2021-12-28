using VeldridGen.Interfaces;

namespace UAlbion.Core.Veldrid.Etm;

[VertexShader(typeof(EtmVertexShader))]
[FragmentShader(typeof(EtmFragmentShader))]
partial class EtmPipeline : PipelineHolder
{
}