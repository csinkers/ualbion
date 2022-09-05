using Veldrid;
namespace UAlbion.Core.Veldrid.Meshes
{
    internal partial class MeshResourceSet
    {
        public static readonly ResourceLayoutDescription Layout = new(
            new ResourceLayoutElementDescription("Diffuse", global::Veldrid.ResourceKind.TextureReadOnly, (ShaderStages)16),
            new ResourceLayoutElementDescription("Sampler", global::Veldrid.ResourceKind.Sampler, (ShaderStages)16),
            new ResourceLayoutElementDescription("MeshUniform", global::Veldrid.ResourceKind.UniformBuffer, (ShaderStages)16));

        public global::VeldridGen.Interfaces.ITextureHolder Diffuse
        {
            get => _diffuse;
            set
            {
                if (_diffuse == value) return;

                if (_diffuse != null)
                    _diffuse.PropertyChanged -= PropertyDirty;

                _diffuse = value;

                if (_diffuse != null)
                    _diffuse.PropertyChanged += PropertyDirty;
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

        public global::VeldridGen.Interfaces.IBufferHolder<global::UAlbion.Core.Veldrid.Meshes.MeshUniform> Uniform
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
                if (_diffuse.DeviceTexture == null) throw new System.InvalidOperationException("Tried to construct MeshResourceSet, but Diffuse has not been initialised. It may not have been attached to the exchange.");
                if (_sampler.Sampler == null) throw new System.InvalidOperationException("Tried to construct MeshResourceSet, but Sampler has not been initialised. It may not have been attached to the exchange.");
                if (_uniform.DeviceBuffer == null) throw new System.InvalidOperationException("Tried to construct MeshResourceSet, but Uniform has not been initialised. It may not have been attached to the exchange.");
#endif

            return device.ResourceFactory.CreateResourceSet(new ResourceSetDescription(
                layout,
                _diffuse.DeviceTexture,
                _sampler.Sampler,
                _uniform.DeviceBuffer));
        }

        protected override void Resubscribe()
        {
            if (_diffuse != null)
                _diffuse.PropertyChanged += PropertyDirty;
            if (_sampler != null)
                _sampler.PropertyChanged += PropertyDirty;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (_diffuse != null)
                _diffuse.PropertyChanged -= PropertyDirty;
            if (_sampler != null)
                _sampler.PropertyChanged -= PropertyDirty;
        }
    }
}
