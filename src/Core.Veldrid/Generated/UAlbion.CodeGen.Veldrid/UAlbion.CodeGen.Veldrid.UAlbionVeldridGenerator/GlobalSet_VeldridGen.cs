using Veldrid;
namespace UAlbion.Core.Veldrid
{
    public partial class GlobalSet
    {
        public static readonly ResourceLayoutDescription Layout = new(
            new ResourceLayoutElementDescription("_Shared", global::Veldrid.ResourceKind.UniformBuffer, (ShaderStages)17),
            new ResourceLayoutElementDescription("uDayPalette", global::Veldrid.ResourceKind.TextureReadOnly, (ShaderStages)16),
            new ResourceLayoutElementDescription("uNightPalette", global::Veldrid.ResourceKind.TextureReadOnly, (ShaderStages)16),
            new ResourceLayoutElementDescription("uPaletteSampler", global::Veldrid.ResourceKind.Sampler, (ShaderStages)16));

        public global::UAlbion.Core.Veldrid.SingleBuffer<global::UAlbion.Core.Veldrid.GlobalInfo> Global
        {
            get => _global;
            set
            {
                if (_global == value)
                    return;
                _global = value;
                Dirty();
            }
        }

        public global::VeldridGen.Interfaces.ITextureHolder DayPalette
        {
            get => _dayPalette;
            set
            {
                if (_dayPalette == value) return;

                if (_dayPalette != null)
                    _dayPalette.PropertyChanged -= PropertyDirty;

                _dayPalette = value;

                if (_dayPalette != null)
                    _dayPalette.PropertyChanged += PropertyDirty;
                Dirty();
            }
        }

        public global::VeldridGen.Interfaces.ITextureHolder NightPalette
        {
            get => _nightPalette;
            set
            {
                if (_nightPalette == value) return;

                if (_nightPalette != null)
                    _nightPalette.PropertyChanged -= PropertyDirty;

                _nightPalette = value;

                if (_nightPalette != null)
                    _nightPalette.PropertyChanged += PropertyDirty;
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

        protected override ResourceSet Build(GraphicsDevice device, ResourceLayout layout)
        {
#if DEBUG
                if (_global.DeviceBuffer == null) throw new System.InvalidOperationException("Tried to construct GlobalSet, but Global has not been initialised. It may not have been attached to the exchange.");
                if (_dayPalette.DeviceTexture == null) throw new System.InvalidOperationException("Tried to construct GlobalSet, but DayPalette has not been initialised. It may not have been attached to the exchange.");
                if (_nightPalette.DeviceTexture == null) throw new System.InvalidOperationException("Tried to construct GlobalSet, but NightPalette has not been initialised. It may not have been attached to the exchange.");
                if (_sampler.Sampler == null) throw new System.InvalidOperationException("Tried to construct GlobalSet, but Sampler has not been initialised. It may not have been attached to the exchange.");
#endif

            return device.ResourceFactory.CreateResourceSet(new ResourceSetDescription(
                layout,
                _global.DeviceBuffer,
                _dayPalette.DeviceTexture,
                _nightPalette.DeviceTexture,
                _sampler.Sampler));
        }

        protected override void Resubscribe()
        {
            if (_dayPalette != null)
                _dayPalette.PropertyChanged += PropertyDirty;
            if (_nightPalette != null)
                _nightPalette.PropertyChanged += PropertyDirty;
            if (_sampler != null)
                _sampler.PropertyChanged += PropertyDirty;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (_dayPalette != null)
                _dayPalette.PropertyChanged -= PropertyDirty;
            if (_nightPalette != null)
                _nightPalette.PropertyChanged -= PropertyDirty;
            if (_sampler != null)
                _sampler.PropertyChanged -= PropertyDirty;
        }
    }
}
