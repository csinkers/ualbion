using System;
using UAlbion.Core.Events;
using UAlbion.Core.Visual;
using Veldrid;

namespace UAlbion.Core.Veldrid.Visual
{
    public class FramebufferSource : VeldridComponent, IFramebufferSource
    {
        public uint Width { get => _width; set { if (_width == value) return;  _width = value; _dirty = true; } } 
        public uint Height { get => _height; set { if (_height == value) return; _height = value; _dirty = true; } } 
        Texture _depth;
        Texture _color;
        uint _width;
        uint _height;
        bool _dirty;

        public FramebufferSource(uint width, uint height)
        {
            On<BackendChangedEvent>(_ => DestroyDeviceObjects());
            _width = width;
            _height = height;
        }

        public int Version { get; private set; }
        public Framebuffer Framebuffer { get; private set; }
        public override void CreateDeviceObjects(VeldridRendererContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (_dirty) DestroyDeviceObjects();
            if (Framebuffer != null) return;

            _depth = context.GraphicsDevice.ResourceFactory.CreateTexture(new TextureDescription(
                Width, Height, 1, 1, 1,
                PixelFormat.R32_Float, TextureUsage.DepthStencil, TextureType.Texture2D));

            _color = context.GraphicsDevice.ResourceFactory.CreateTexture(new TextureDescription(
                Width, Height, 1, 1, 1,
                PixelFormat.B8_G8_R8_A8_UNorm, TextureUsage.RenderTarget, TextureType.Texture2D));

            var description = new FramebufferDescription(_depth, _color);
            Framebuffer = context.GraphicsDevice.ResourceFactory.CreateFramebuffer(ref description);
            Version++;
            _dirty = false;
        }

        public override void DestroyDeviceObjects()
        {
            if (Framebuffer == null)
                return;

            Framebuffer.Dispose();
            _depth.Dispose();
            _color.Dispose();
            Framebuffer = null;
            _depth = null;
            _color = null;
        }
    }
}