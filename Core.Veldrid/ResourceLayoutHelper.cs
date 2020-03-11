using Veldrid;

namespace UAlbion.Core.Veldrid
{
    public static class ResourceLayoutHelper
    {
        public static ResourceLayoutElementDescription UniformV(string name) => new ResourceLayoutElementDescription(name, ResourceKind.UniformBuffer, ShaderStages.Vertex);
        public static ResourceLayoutElementDescription UniformF(string name) => new ResourceLayoutElementDescription(name, ResourceKind.UniformBuffer, ShaderStages.Fragment);
        public static ResourceLayoutElementDescription Uniform(string name) => new ResourceLayoutElementDescription(name, ResourceKind.UniformBuffer, ShaderStages.Vertex | ShaderStages.Fragment);
        public static ResourceLayoutElementDescription Texture(string name) => new ResourceLayoutElementDescription(name, ResourceKind.TextureReadOnly, ShaderStages.Fragment);
        public static ResourceLayoutElementDescription Sampler(string name) => new ResourceLayoutElementDescription(name, ResourceKind.Sampler, ShaderStages.Fragment);
    }
}
