using System;
using System.ComponentModel;
using UAlbion.Api.Visual;
using UAlbion.Core.Veldrid.Events;
using Veldrid;

namespace UAlbion.Core.Veldrid
{
    public abstract class TextureHolder : Component
    {
        ITexture _texture;
        Texture _deviceTexture;

        public DateTime LastAccessDateTime { get; private set; }
        public string Name => Texture.Name;
        public ITexture Texture
        {
            get => _texture;
            set
            {
                if (_texture == value)
                    return;
                _texture = value;
                Dirty();
            }
        }

        public Texture DeviceTexture
        {
            get
            {
                LastAccessDateTime = DateTime.Now;
                return _deviceTexture;
            }
            private set
            {
                if (_deviceTexture == value)
                    return;
                _deviceTexture = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DeviceTexture)));
            }
        }

        protected TextureHolder(ITexture texture)
        {
            _texture = texture ?? throw new ArgumentNullException(nameof(texture));
            On<DeviceCreatedEvent>(_ => Dirty());
            On<DestroyDeviceObjectsEvent>(_ => Dispose());
        }

        protected override void Subscribed() => Dirty();
        protected override void Unsubscribed() => Dispose();
        void Dirty() => On<PrepareFrameResourcesEvent>(e =>
        {
            Dispose();
            DeviceTexture = Create(e.Device);
            Off<PrepareFrameResourcesEvent>();
        });
        protected abstract Texture Create(GraphicsDevice device);

        public void Dispose()
        {
            DeviceTexture?.Dispose();
            DeviceTexture = null;
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}