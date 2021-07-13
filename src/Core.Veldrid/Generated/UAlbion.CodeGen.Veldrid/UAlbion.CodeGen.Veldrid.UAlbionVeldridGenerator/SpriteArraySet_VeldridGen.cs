using Veldrid;
namespace UAlbion.Core.Veldrid.Sprites
{
    internal partial class SpriteArraySet
    {
        public static readonly ResourceLayoutDescription Layout = new(
            new ResourceLayoutElementDescription("uSprite", global::Veldrid.ResourceKind.TextureReadOnly, (ShaderStages)17),
            new ResourceLayoutElementDescription("uSpriteSampler", global::Veldrid.ResourceKind.Sampler, (ShaderStages)17),
            new ResourceLayoutElementDescription("_Uniform", global::Veldrid.ResourceKind.UniformBuffer, (ShaderStages)17));

        public global::VeldridGen.Interfaces.ITextureArrayHolder Texture
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

        public global::VeldridGen.Interfaces.IBufferHolder<global::UAlbion.Core.Veldrid.Sprites.SpriteUniform> Uniform
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
                _texture.DeviceTexture,
                _sampler.Sampler,
                _uniform.DeviceBuffer));

        protected override void Resubscribe()
        {
            if (_texture != null)
                _texture.PropertyChanged += PropertyDirty;
            if (_sampler != null)
                _sampler.PropertyChanged += PropertyDirty;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (_texture != null)
                _texture.PropertyChanged -= PropertyDirty;
            if (_sampler != null)
                _sampler.PropertyChanged -= PropertyDirty;
        }
    }
}
