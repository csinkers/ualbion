using System.ComponentModel;
using UAlbion.Core.Veldrid.Events;
using Veldrid;
using VeldridGen.Interfaces;

namespace UAlbion.Core.Veldrid
{
    public abstract class FramebufferHolder : Component, IFramebufferHolder
    {
        uint _width;
        uint _height;
        Framebuffer _framebuffer;

        public uint Width { get => _width; set { if (_width == value) return;  _width = value; Dirty(); } } 
        public uint Height { get => _height; set { if (_height == value) return; _height = value; Dirty(); } }

        public Framebuffer Framebuffer
        {
            get => _framebuffer;
            protected set
            {
                _framebuffer = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Framebuffer)));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void Dispose() => Dispose(true);

        protected FramebufferHolder(uint width, uint height)
        {
            On<DeviceCreatedEvent>(_ => Dirty());
            On<DestroyDeviceObjectsEvent>(_ => Dispose());
            _width = width;
            _height = height;
        }

        protected override void Subscribed() => Dirty();
        protected override void Unsubscribed() => Dispose();
        protected abstract Framebuffer CreateFramebuffer(GraphicsDevice device);
        public abstract OutputDescription? OutputDescription { get; }
        protected virtual void Dispose(bool disposing)
        {
            Framebuffer?.Dispose();
            Framebuffer = null;
        }

        void Dirty() => On<PrepareFrameResourcesEvent>(e =>
        {
            Dispose();
            Framebuffer = CreateFramebuffer(e.Device);
            _width = Framebuffer.Width;
            _height = Framebuffer.Height;
            Off<PrepareFrameResourcesEvent>();
        });
    }
}
