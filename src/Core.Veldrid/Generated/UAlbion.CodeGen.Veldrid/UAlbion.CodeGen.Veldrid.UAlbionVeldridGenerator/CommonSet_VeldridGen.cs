using Veldrid;
namespace UAlbion.Core.Veldrid
{
    public partial class CommonSet
    {
        public static readonly ResourceLayoutDescription Layout = new(
            new ResourceLayoutElementDescription("_Shared", global::Veldrid.ResourceKind.UniformBuffer, (ShaderStages)17),
            new ResourceLayoutElementDescription("_Projection", global::Veldrid.ResourceKind.UniformBuffer, (ShaderStages)1),
            new ResourceLayoutElementDescription("_View", global::Veldrid.ResourceKind.UniformBuffer, (ShaderStages)1),
            new ResourceLayoutElementDescription("uPalette", global::Veldrid.ResourceKind.TextureReadOnly, (ShaderStages)16));

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

        public global::VeldridGen.Interfaces.ITextureHolder Palette
        {
            get => _palette;
            set
            {
                if (_palette == value) return;

                if (_palette != null)
                    _palette.PropertyChanged -= PropertyDirty;

                _palette = value;

                if (_palette != null)
                    _palette.PropertyChanged += PropertyDirty;
                Dirty();
            }
        }

        protected override ResourceSet Build(GraphicsDevice device, ResourceLayout layout) =>
            device.ResourceFactory.CreateResourceSet(new ResourceSetDescription(
                layout,
                _globalInfo.DeviceBuffer,
                _projection.DeviceBuffer,
                _view.DeviceBuffer,
                _palette.DeviceTexture));

        protected override void Resubscribe()
        {
            if (_palette != null)
                _palette.PropertyChanged += PropertyDirty;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (_palette != null)
                _palette.PropertyChanged -= PropertyDirty;
        }
    }
}
