using Veldrid;
namespace UAlbion.Core.Veldrid.Sprites
{
    internal partial class TileLayerResourceSet
    {
        public static readonly ResourceLayoutDescription Layout = new(
            new ResourceLayoutElementDescription("_LayerUniform", global::Veldrid.ResourceKind.UniformBuffer, (ShaderStages)17),
            new ResourceLayoutElementDescription("MapBuffer", global::Veldrid.ResourceKind.StructuredBufferReadOnly, (ShaderStages)16));

        public global::VeldridGen.Interfaces.IBufferHolder<global::UAlbion.Core.Veldrid.Sprites.TileLayerUniform> Uniform
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

        public global::VeldridGen.Interfaces.IBufferHolder<global::UAlbion.Core.Veldrid.Sprites.GpuMapTile> Map
        {
            get => _map;
            set
            {
                if (_map == value)
                    return;
                _map = value;
                Dirty();
            }
        }

        protected override ResourceSet Build(GraphicsDevice device, ResourceLayout layout)
        {
#if DEBUG
                if (_uniform.DeviceBuffer == null) throw new System.InvalidOperationException("Tried to construct TileLayerResourceSet without setting Uniform to a non-null value");
                if (_map.DeviceBuffer == null) throw new System.InvalidOperationException("Tried to construct TileLayerResourceSet without setting Map to a non-null value");
#endif

            return device.ResourceFactory.CreateResourceSet(new ResourceSetDescription(
                layout,
                _uniform.DeviceBuffer,
                _map.DeviceBuffer));
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
