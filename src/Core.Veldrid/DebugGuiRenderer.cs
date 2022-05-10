using System;
using UAlbion.Api.Eventing;
using UAlbion.Core.Events;
using UAlbion.Core.Veldrid.Events;
using UAlbion.Core.Visual;
using Veldrid;
using VeldridGen.Interfaces;

namespace UAlbion.Core.Veldrid;

public sealed class DebugGuiRenderer : Component, IRenderer, IDisposable
{
    readonly IFramebufferHolder _framebuffer;
    ImGuiRenderer _imguiRenderer;

    public Type[] HandledTypes { get; } = { typeof(DebugGuiRenderable) };
    public DebugGuiRenderer(IFramebufferHolder framebuffer)
    {
        _framebuffer = framebuffer;
        On<InputEvent>(e => _imguiRenderer?.Update((float)e.DeltaSeconds, e.Snapshot));
        On<WindowResizedEvent>(e => _imguiRenderer?.WindowResized(e.Width, e.Height));
        On<DeviceCreatedEvent>(_ => Dirty());
        On<DestroyDeviceObjectsEvent>(_ => Dispose());
    }

    protected override void Subscribed() => Dirty();
    protected override void Unsubscribed() => Dispose();
    void Dirty() => On<PrepareFrameResourcesEvent>(e => CreateDeviceObjects(e.Device));

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
        Off<PrepareFrameResourcesEvent>();
    }

    public void Dispose()
    {
        _imguiRenderer?.Dispose();
        _imguiRenderer = null;
    }

    public void Render(IRenderable renderable, CommonSet commonSet, IFramebufferHolder framebuffer, CommandList cl, GraphicsDevice device)
    {
        if (cl == null) throw new ArgumentNullException(nameof(cl));
        if (device == null) throw new ArgumentNullException(nameof(device));
        if (renderable is not DebugGuiRenderable)
            throw new ArgumentException($"{GetType().Name} was passed renderable of unexpected type {renderable?.GetType().Name ?? "null"}", nameof(renderable));

        _imguiRenderer.Render(device, cl);
        cl.SetFullScissorRects();
    }
}