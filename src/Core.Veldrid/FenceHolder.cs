using System.ComponentModel;
using UAlbion.Core.Veldrid.Events;
using Veldrid;
using VeldridGen.Interfaces;

namespace UAlbion.Core.Veldrid
{
    public sealed class FenceHolder : Component, IFenceHolder
    {
        Fence _fence;

        public string Name { get; init; }
        public Fence Fence
        {
            get => _fence;
            private set
            {
                _fence = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Fence)));
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        public FenceHolder(string name)
        {
            Name = name;
            On<DeviceCreatedEvent>(e => Update(e.Device));
            On<DestroyDeviceObjectsEvent>(_ => Dispose());
        }

        protected override void Subscribed() => On<PrepareFrameResourcesEvent>(e => Update(e.Device));
        protected override void Unsubscribed() => Dispose();
        public void Dispose()
        {
            Fence?.Dispose();
            Fence = null;
        }

        void Update(GraphicsDevice device)
        {
            Dispose();
            Fence = device.ResourceFactory.CreateFence(false);
            Off<PrepareFrameResourcesEvent>();
        }
    }
}