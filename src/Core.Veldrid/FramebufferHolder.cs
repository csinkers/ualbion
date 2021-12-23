using System;
using System.Collections.Generic;
using System.ComponentModel;
using UAlbion.Core.Veldrid.Events;
using Veldrid;
using VeldridGen.Interfaces;

namespace UAlbion.Core.Veldrid
{
    public abstract class FramebufferHolder : Component, IFramebufferHolder
    {
        class Holder : ITextureHolder
        {
            readonly Func<Texture> _getter;
            public Holder(Func<Texture> getter) => _getter = getter ?? throw new ArgumentNullException(nameof(getter));
            public event PropertyChangedEventHandler PropertyChanged;
            public Texture DeviceTexture => _getter();
            public string Name => _getter()?.Name;
        }

        readonly object _syncRoot = new();
        readonly List<Holder> _colorHolders = new();
        Framebuffer _framebuffer;
        Holder _depthHolder;
        uint _width;
        uint _height;

        public string Name { get; }
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
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected FramebufferHolder(uint width, uint height, string name)
        {
            On<DeviceCreatedEvent>(e => Update(e.Device));
            On<DestroyDeviceObjectsEvent>(_ => Dispose());
            _width = width;
            _height = height;
            Name = name;
        }

        protected override void Subscribed() => Dirty();
        protected override void Unsubscribed() => Dispose();
        protected abstract Framebuffer CreateFramebuffer(GraphicsDevice device);
        public abstract OutputDescription? OutputDescription { get; }

        public ITextureHolder DepthTexture
        {
            get
            {
                lock (_syncRoot)
                    return _depthHolder ??= new Holder(() => Framebuffer.DepthTarget?.Target);
            }
        }

        public ITextureHolder GetColorTexture(int index)
        {
            lock (_syncRoot)
            {
                while(_colorHolders.Count <= index)
                    _colorHolders.Add(new Holder(() => Framebuffer.ColorTargets[index].Target));
                return _colorHolders[index];
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            Framebuffer?.Dispose();
            Framebuffer = null;
        }

        void Dirty() => On<PrepareFrameResourcesEvent>(e => Update(e.Device));
        void Update(GraphicsDevice device)
        {
            Dispose();
            Framebuffer = CreateFramebuffer(device);
            _width = Framebuffer.Width;
            _height = Framebuffer.Height;
            Off<PrepareFrameResourcesEvent>();
        }
    }
}
