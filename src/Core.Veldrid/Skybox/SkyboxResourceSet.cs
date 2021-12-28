using Veldrid;
using VeldridGen.Interfaces;

namespace UAlbion.Core.Veldrid.Skybox;

partial class SkyboxResourceSet : ResourceSetHolder
{
    [Resource("uSampler", ShaderStages.Fragment)] ISamplerHolder _sampler;
    [Resource("uTexture", ShaderStages.Fragment)] ITextureHolder _texture;
    [Resource("_Uniform", ShaderStages.Vertex)] IBufferHolder<SkyboxUniformInfo> _uniform;
}