using System;
using System.ComponentModel;
using UAlbion.Api.Visual;
using UAlbion.Core.Veldrid.Events;
using Veldrid;
using VeldridGen.Interfaces;

namespace UAlbion.Core.Veldrid
{
    public class Texture2DHolder : Component, ITextureHolder
    {
        ITexture _texture;
        Texture _deviceTexture;

        public DateTime LastAccessDateTime { get; private set; }
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

        public Texture2DHolder(ITexture texture)
        {
            _texture = texture ?? throw new ArgumentNullException(nameof(texture));
            On<DestroyDeviceObjectsEvent>(_ => Dispose());
        }

        protected override void Subscribed() => Dirty();
        protected override void Unsubscribed() => Dispose();
        void Dirty() => On<PrepareFrameResourcesEvent>(Update);

        void Update(PrepareFrameResourcesEvent e)
        {
            Dispose();
            DeviceTexture = Texture switch
                { // Note: No automatic mip-mapping for 8-bit, blending/interpolation in palette-based images typically results in nonsense.
                  // TODO: Custom mip-mapping using nearest matches in the palette
                    IReadOnlyTexture<byte> eightBit => VeldridTexture.CreateSimpleTexture(e.Device, TextureUsage.Sampled, eightBit),
                    IReadOnlyTexture<uint> trueColor => VeldridTexture.CreateSimpleTexture(e.Device, TextureUsage.Sampled | TextureUsage.GenerateMipmaps, trueColor),
                    _ => throw new NotSupportedException($"Image format {Texture.GetType().GetGenericArguments()[0].Name} not currently supported")
                };

            Off<PrepareFrameResourcesEvent>();
        }

        public void Dispose()
        {
            DeviceTexture?.Dispose();
            DeviceTexture = null;
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
