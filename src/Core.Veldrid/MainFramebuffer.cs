using System;
using UAlbion.Core.Events;
using Veldrid;

namespace UAlbion.Core.Veldrid
{
    public class MainFramebuffer : FramebufferHolder
    {
        public MainFramebuffer() : base(0, 0, "FB_Main")
        {
            On<WindowResizedEvent>(e =>
            {
                Width = (uint)e.Width;
                Height = (uint)e.Height;
            });
        }

        protected override void Dispose(bool disposing)
        {
            Framebuffer = null; // Main frame buffer is owned by GraphicsDevice
        }

        protected override Framebuffer CreateFramebuffer(GraphicsDevice device)
        {
            if (device == null) throw new ArgumentNullException(nameof(device));
            return device.SwapchainFramebuffer;
        }

        public override OutputDescription? OutputDescription => Framebuffer?.OutputDescription;
    }
}