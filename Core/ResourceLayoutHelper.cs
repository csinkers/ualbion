using Veldrid;

namespace UAlbion.Core
{
    public static class ResourceLayoutHelper
    {
        public static ResourceLayoutElementDescription Uniform(string name) => new ResourceLayoutElementDescription(name, ResourceKind.UniformBuffer, ShaderStages.Vertex);
        public static ResourceLayoutElementDescription Texture(string name) => new ResourceLayoutElementDescription(name, ResourceKind.TextureReadOnly, ShaderStages.Fragment);
        public static ResourceLayoutElementDescription Sampler(string name) => new ResourceLayoutElementDescription(name, ResourceKind.Sampler, ShaderStages.Fragment);
    }
}