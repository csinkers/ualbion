using Veldrid;
namespace UAlbion.Core.Veldrid.Meshes
{
    internal partial class MeshSet
    {
        public static readonly ResourceLayoutDescription Layout = new(
            new ResourceLayoutElementDescription("TextureSampler", global::Veldrid.ResourceKind.Sampler, (ShaderStages)16));

        public global::VeldridGen.Interfaces.ISamplerHolder TextureSampler
        {
            get => _textureSampler;
            set
            {
                if (_textureSampler == value) 
                    return;

                if (_textureSampler != null)
                    _textureSampler.PropertyChanged -= PropertyDirty;

                _textureSampler = value;

                if (_textureSampler != null)
                    _textureSampler.PropertyChanged += PropertyDirty;
                Dirty();
            }
        }

        protected override ResourceSet Build(GraphicsDevice device, ResourceLayout layout)
        {
#if DEBUG
                if (_textureSampler.Sampler == null) throw new System.InvalidOperationException("Tried to construct MeshSet without setting TextureSampler to a non-null value");
#endif

            return device.ResourceFactory.CreateResourceSet(new ResourceSetDescription(
                layout,
                _textureSampler.Sampler));
        }

        protected override void Resubscribe()
        {
            if (_textureSampler != null)
                _textureSampler.PropertyChanged += PropertyDirty;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (_textureSampler != null)
                _textureSampler.PropertyChanged -= PropertyDirty;
        }
    }
}
