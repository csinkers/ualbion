using System.ComponentModel;
using UAlbion.Core.Veldrid.Events;
using Veldrid;
using VeldridGen.Interfaces;

namespace UAlbion.Core.Veldrid
{
    public abstract class ResourceSetHolder : Component, IResourceSetHolder
    {
        ResourceSet _resourceSet;
        string _name;

        protected ResourceSetHolder()
        {
            On<DeviceCreatedEvent>(_ => Dirty());
            On<DestroyDeviceObjectsEvent>(_ => Dispose());
        }

        public ResourceSet ResourceSet => _resourceSet;

        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                if (_resourceSet != null)
                    _resourceSet.Name = _name;
            }
        }

        protected override void Subscribed() => Dirty();
        protected override void Unsubscribed() => Dispose();
        protected void Dirty() => On<PrepareFrameResourceSetsEvent>(Update);
        protected void PropertyDirty(object sender, PropertyChangedEventArgs _) => Dirty();
        protected abstract ResourceSet Build(GraphicsDevice device, ResourceLayout layout);

        void Update(IVeldridInitEvent e)
        {
            if (_resourceSet != null)
                Dispose();

            var layoutSource = Resolve<IResourceLayoutSource>();
            var layout = layoutSource.Get(GetType(), e.Device);
            _resourceSet = Build(e.Device, layout);
            _resourceSet.Name = Name;
            Off<PrepareFrameResourceSetsEvent>();
        }

        public void Dispose()
        {
            _resourceSet?.Dispose();
            _resourceSet = null;
        }
    }
}
