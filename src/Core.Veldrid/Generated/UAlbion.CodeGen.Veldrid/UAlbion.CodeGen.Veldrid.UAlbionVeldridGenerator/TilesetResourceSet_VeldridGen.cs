using Veldrid;
namespace UAlbion.Core.Veldrid.Sprites
{
    internal partial class TilesetResourceSet
    {
        public static readonly ResourceLayoutDescription Layout = new(
            new ResourceLayoutElementDescription("uTile", global::Veldrid.ResourceKind.TextureReadOnly, (ShaderStages)16),
            new ResourceLayoutElementDescription("uTileArray", global::Veldrid.ResourceKind.TextureReadOnly, (ShaderStages)16),
            new ResourceLayoutElementDescription("uTileSampler", global::Veldrid.ResourceKind.Sampler, (ShaderStages)16),
            new ResourceLayoutElementDescription("_SetUniform", global::Veldrid.ResourceKind.UniformBuffer, (ShaderStages)17),
            new ResourceLayoutElementDescription("TilesBuffer", global::Veldrid.ResourceKind.StructuredBufferReadOnly, (ShaderStages)16),
            new ResourceLayoutElementDescription("RegionsBuffer", global::Veldrid.ResourceKind.StructuredBufferReadOnly, (ShaderStages)16));

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

        public global::VeldridGen.Interfaces.IBufferHolder<global::UAlbion.Core.Veldrid.Sprites.TilesetUniform> Uniform
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

        public global::VeldridGen.Interfaces.IBufferHolder<global::UAlbion.Core.Veldrid.Sprites.GpuTileData> Tiles
        {
            get => _tiles;
            set
            {
                if (_tiles == value)
                    return;
                _tiles = value;
                Dirty();
            }
        }

        public global::VeldridGen.Interfaces.IBufferHolder<global::UAlbion.Core.Veldrid.Sprites.GpuTextureRegion> Regions
        {
            get => _regions;
            set
            {
                if (_regions == value)
                    return;
                _regions = value;
                Dirty();
            }
        }

        protected override ResourceSet Build(GraphicsDevice device, ResourceLayout layout)
        {
#if DEBUG
                if (_texture.DeviceTexture == null) throw new System.InvalidOperationException("Tried to construct TilesetResourceSet without setting Texture to a non-null value");
                if (_textureArray.DeviceTexture == null) throw new System.InvalidOperationException("Tried to construct TilesetResourceSet without setting TextureArray to a non-null value");
                if (_sampler.Sampler == null) throw new System.InvalidOperationException("Tried to construct TilesetResourceSet without setting Sampler to a non-null value");
                if (_uniform.DeviceBuffer == null) throw new System.InvalidOperationException("Tried to construct TilesetResourceSet without setting Uniform to a non-null value");
                if (_tiles.DeviceBuffer == null) throw new System.InvalidOperationException("Tried to construct TilesetResourceSet without setting Tiles to a non-null value");
                if (_regions.DeviceBuffer == null) throw new System.InvalidOperationException("Tried to construct TilesetResourceSet without setting Regions to a non-null value");
#endif

            return device.ResourceFactory.CreateResourceSet(new ResourceSetDescription(
                layout,
                _texture.DeviceTexture,
                _textureArray.DeviceTexture,
                _sampler.Sampler,
                _uniform.DeviceBuffer,
                _tiles.DeviceBuffer,
                _regions.DeviceBuffer));
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
