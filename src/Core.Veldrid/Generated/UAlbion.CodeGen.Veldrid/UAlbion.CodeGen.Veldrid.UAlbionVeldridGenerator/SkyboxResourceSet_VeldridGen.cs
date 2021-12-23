using Veldrid;
namespace UAlbion.Core.Veldrid.Skybox
{
    internal partial class SkyboxResourceSet
    {
        public static readonly ResourceLayoutDescription Layout = new(
            new ResourceLayoutElementDescription("uSampler", global::Veldrid.ResourceKind.Sampler, (ShaderStages)16),
            new ResourceLayoutElementDescription("uTexture", global::Veldrid.ResourceKind.TextureReadOnly, (ShaderStages)16),
            new ResourceLayoutElementDescription("_Uniform", global::Veldrid.ResourceKind.UniformBuffer, (ShaderStages)1));

        public global::VeldridGen.Interfaces.ISamplerHolder Sampler
        {
            get => _sampler;
            set
            {
                if (_sampler == value) 
                    return;

                if (_sampler != null)
                    _sampler.PropertyChanged -= PropertyDirty;

                _sampler = value;

                if (_sampler != null)
                    _sampler.PropertyChanged += PropertyDirty;
                Dirty();
            }
        }

        public global::VeldridGen.Interfaces.ITextureHolder Texture
        {
            get => _texture;
            set
            {
                if (_texture == value) return;

                if (_texture != null)
                    _texture.PropertyChanged -= PropertyDirty;

                _texture = value;

                if (_texture != null)
                    _texture.PropertyChanged += PropertyDirty;
                Dirty();
            }
        }

        public global::VeldridGen.Interfaces.IBufferHolder<global::UAlbion.Core.Veldrid.Skybox.SkyboxUniformInfo> Uniform
        {
            get => _uniform;
            set
            {
                if (_uniform == value)
                    return;
                _uniform = value;
                Dirty();
            }
        }

        protected override ResourceSet Build(GraphicsDevice device, ResourceLayout layout) =>
            device.ResourceFactory.CreateResourceSet(new ResourceSetDescription(
                layout,
                _sampler.Sampler,
                _texture.DeviceTexture,
                _uniform.DeviceBuffer));

        protected override void Resubscribe()
        {
            if (_sampler != null)
                _sampler.PropertyChanged += PropertyDirty;
            if (_texture != null)
                _texture.PropertyChanged += PropertyDirty;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (_sampler != null)
                _sampler.PropertyChanged -= PropertyDirty;
            if (_texture != null)
                _texture.PropertyChanged -= PropertyDirty;
        }
    }
}
