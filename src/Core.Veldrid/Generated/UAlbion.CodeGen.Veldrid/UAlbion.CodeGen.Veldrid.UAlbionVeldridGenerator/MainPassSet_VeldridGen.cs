using Veldrid;
namespace UAlbion.Core.Veldrid
{
    public partial class MainPassSet
    {
        public static readonly ResourceLayoutDescription Layout = new(
            new ResourceLayoutElementDescription("_Camera", global::Veldrid.ResourceKind.UniformBuffer, (ShaderStages)17));

        public global::UAlbion.Core.Veldrid.SingleBuffer<global::UAlbion.Core.Veldrid.CameraUniform> Camera
        {
            get => _camera;
            set
            {
                if (_camera == value)
                    return;
                _camera = value;
                Dirty();
            }
        }

        protected override ResourceSet Build(GraphicsDevice device, ResourceLayout layout)
        {
#if DEBUG
                if (_camera.DeviceBuffer == null) throw new System.InvalidOperationException("Tried to construct MainPassSet, but Camera has not been initialised. It may not have been attached to the exchange.");
#endif

            return device.ResourceFactory.CreateResourceSet(new ResourceSetDescription(
                layout,
                _camera.DeviceBuffer));
        }

        protected override void Resubscribe()
        {
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }
    }
}
