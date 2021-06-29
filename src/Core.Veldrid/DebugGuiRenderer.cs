using System;
using UAlbion.Core.Events;
using UAlbion.Core.Veldrid.Events;
using Veldrid;
using VeldridGen.Interfaces;

namespace UAlbion.Core.Veldrid
{
    public sealed class DebugGuiRenderer : Component, IDisposable
    {
        readonly IFramebufferHolder _framebuffer;
        ImGuiRenderer _imguiRenderer;

        public DebugGuiRenderer(IFramebufferHolder framebuffer)
        {
            _framebuffer = framebuffer;
            On<InputEvent>(e => _imguiRenderer?.Update((float)e.DeltaSeconds, e.Snapshot));
            On<WindowResizedEvent>(e => _imguiRenderer?.WindowResized(e.Width, e.Height));
            On<DestroyDeviceObjectsEvent>(_ => Dispose());
        }

        protected override void Subscribed() => Dirty();
        protected override void Unsubscribed() => Dispose();
        void Dirty() => On<PostEngineUpdateEvent>(e => CreateDeviceObjects(e.Device));

        void CreateDeviceObjects(GraphicsDevice graphicsDevice)
        {
            if (graphicsDevice == null) throw new ArgumentNullException(nameof(graphicsDevice));
            if (_imguiRenderer == null)
            {
                var window = Resolve<IWindowManager>();
                _imguiRenderer = new ImGuiRenderer(
                    graphicsDevice,
                    _framebuffer.Framebuffer.OutputDescription,
                    window.PixelWidth,
                    window.PixelHeight,
                    ColorSpaceHandling.Linear);
            }
            else
            {
                _imguiRenderer.CreateDeviceResources(graphicsDevice, graphicsDevice.SwapchainFramebuffer.OutputDescription, ColorSpaceHandling.Linear);
            }
            Off<PostEngineUpdateEvent>();
        }

        public void Render(GraphicsDevice gd, CommandList cl)
        {
            if (gd == null) throw new ArgumentNullException(nameof(gd));
            if (cl == null) throw new ArgumentNullException(nameof(cl));
            _imguiRenderer.Render(gd, cl);
            cl.SetFullScissorRects();
        }

        public void Dispose()
        {
            _imguiRenderer?.Dispose();
            _imguiRenderer = null;
        }
    }
}
