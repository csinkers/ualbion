using System;
using UAlbion.Core.Events;
using Veldrid;

namespace UAlbion.Core.Veldrid;

public class MainFramebuffer : FramebufferHolder
{
    public MainFramebuffer(string name) : base(name, 0, 0)
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
        base.Dispose(disposing);
    }

    protected override Framebuffer CreateFramebuffer(GraphicsDevice device)
    {
        ArgumentNullException.ThrowIfNull(device);
        return device.SwapchainFramebuffer;
    }

    public override OutputDescription? OutputDescription => Framebuffer?.OutputDescription;
}