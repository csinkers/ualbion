using Veldrid;
namespace UAlbion.Core.Veldrid.Sprites
{
    internal partial class SpriteSet
    {
        public static readonly ResourceLayoutDescription Layout = new(
            new ResourceLayoutElementDescription("uSprite", global::Veldrid.ResourceKind.TextureReadOnly, (ShaderStages)17),
            new ResourceLayoutElementDescription("uSpriteArray", global::Veldrid.ResourceKind.TextureReadOnly, (ShaderStages)17),
            new ResourceLayoutElementDescription("uSpriteSampler", global::Veldrid.ResourceKind.Sampler, (ShaderStages)17),
            new ResourceLayoutElementDescription("_Uniform", global::Veldrid.ResourceKind.UniformBuffer, (ShaderStages)17));

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

        public global::VeldridGen.Interfaces.ITextureArrayHolder TextureArray
        {
            get => _textureArray;
            set
            {
                if (_textureArray == value) return;

                if (_textureArray != null)
                    _textureArray.PropertyChanged -= PropertyDirty;

                _textureArray = value;

                if (_textureArray != null)
                    _textureArray.PropertyChanged += PropertyDirty;
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

        protected override ResourceSet Build(GraphicsDevice device, ResourceLayout layout)
        {
#if DEBUG
                if (_texture.DeviceTexture == null) throw new System.InvalidOperationException("Tried to construct SpriteSet without setting Texture to a non-null value");
                if (_textureArray.DeviceTexture == null) throw new System.InvalidOperationException("Tried to construct SpriteSet without setting TextureArray to a non-null value");
                if (_sampler.Sampler == null) throw new System.InvalidOperationException("Tried to construct SpriteSet without setting Sampler to a non-null value");
                if (_uniform.DeviceBuffer == null) throw new System.InvalidOperationException("Tried to construct SpriteSet without setting Uniform to a non-null value");
#endif

            return device.ResourceFactory.CreateResourceSet(new ResourceSetDescription(
                layout,
                _texture.DeviceTexture,
                _textureArray.DeviceTexture,
                _sampler.Sampler,
                _uniform.DeviceBuffer));
        }

        protected override void Resubscribe()
        {
            if (_texture != null)
                _texture.PropertyChanged += PropertyDirty;
            if (_textureArray != null)
                _textureArray.PropertyChanged += PropertyDirty;
            if (_sampler != null)
                _sampler.PropertyChanged += PropertyDirty;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (_texture != null)
                _texture.PropertyChanged -= PropertyDirty;
            if (_textureArray != null)
                _textureArray.PropertyChanged -= PropertyDirty;
            if (_sampler != null)
                _sampler.PropertyChanged -= PropertyDirty;
        }
    }
}
