using Veldrid;
namespace UAlbion.Core.Veldrid
{
    public partial class CommonSet
    {
        public static readonly ResourceLayoutDescription Layout = new(
            new ResourceLayoutElementDescription("_Shared", global::Veldrid.ResourceKind.UniformBuffer, (ShaderStages)17),
            new ResourceLayoutElementDescription("_Projection", global::Veldrid.ResourceKind.UniformBuffer, (ShaderStages)1),
            new ResourceLayoutElementDescription("_View", global::Veldrid.ResourceKind.UniformBuffer, (ShaderStages)1),
            new ResourceLayoutElementDescription("uDayPalette", global::Veldrid.ResourceKind.TextureReadOnly, (ShaderStages)16),
            new ResourceLayoutElementDescription("uNightPalette", global::Veldrid.ResourceKind.TextureReadOnly, (ShaderStages)16),
            new ResourceLayoutElementDescription("uPaletteSampler", global::Veldrid.ResourceKind.Sampler, (ShaderStages)16));

        public global::UAlbion.Core.Veldrid.SingleBuffer<global::UAlbion.Core.Veldrid.GlobalInfo> GlobalInfo
        {
            get => _globalInfo;
            set
            {
                if (_globalInfo == value)
                    return;
                _globalInfo = value;
                Dirty();
            }
        }

        public global::UAlbion.Core.Veldrid.SingleBuffer<global::UAlbion.Core.Veldrid.ProjectionMatrix> Projection
        {
            get => _projection;
            set
            {
                if (_projection == value)
                    return;
                _projection = value;
                Dirty();
            }
        }

        public global::UAlbion.Core.Veldrid.SingleBuffer<global::UAlbion.Core.Veldrid.ViewMatrix> View
        {
            get => _view;
            set
            {
                if (_view == value)
                    return;
                _view = value;
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
                if (_globalInfo.DeviceBuffer == null) throw new System.InvalidOperationException("Tried to construct CommonSet without setting GlobalInfo to a non-null value");
                if (_projection.DeviceBuffer == null) throw new System.InvalidOperationException("Tried to construct CommonSet without setting Projection to a non-null value");
                if (_view.DeviceBuffer == null) throw new System.InvalidOperationException("Tried to construct CommonSet without setting View to a non-null value");
                if (_dayPalette.DeviceTexture == null) throw new System.InvalidOperationException("Tried to construct CommonSet without setting DayPalette to a non-null value");
                if (_nightPalette.DeviceTexture == null) throw new System.InvalidOperationException("Tried to construct CommonSet without setting NightPalette to a non-null value");
                if (_sampler.Sampler == null) throw new System.InvalidOperationException("Tried to construct CommonSet without setting Sampler to a non-null value");
#endif

            return device.ResourceFactory.CreateResourceSet(new ResourceSetDescription(
                layout,
                _globalInfo.DeviceBuffer,
                _projection.DeviceBuffer,
                _view.DeviceBuffer,
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
