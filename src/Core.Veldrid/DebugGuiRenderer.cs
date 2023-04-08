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
    readonly OutputDescription _outputFormat;
    public ImGuiRenderer ImGuiRenderer { get; private set; }

    public Type[] HandledTypes { get; } = { typeof(DebugGuiRenderable) };
    public DebugGuiRenderer(in OutputDescription outputFormat)
    {
        _outputFormat = outputFormat;
        On<WindowResizedEvent>(e => ImGuiRenderer?.WindowResized(e.Width, e.Height));
        On<DeviceCreatedEvent>(_ => Dirty());
        On<DestroyDeviceObjectsEvent>(_ => Dispose());
    }

    protected override void Subscribed() => Dirty();
    protected override void Unsubscribed() => Dispose();
    void Dirty() => On<PrepareFrameResourcesEvent>(e => CreateDeviceObjects(e.Device));

    void CreateDeviceObjects(GraphicsDevice graphicsDevice)
    {
        if (graphicsDevice == null)
            throw new ArgumentNullException(nameof(graphicsDevice));

        if (ImGuiRenderer == null)
        {
            var window = Resolve<IGameWindow>();
            ImGuiRenderer = new ImGuiRenderer(
                graphicsDevice,
                _outputFormat,
                window.PixelWidth,
                window.PixelHeight,
                ColorSpaceHandling.Linear);
        }
        else
        {
            ImGuiRenderer.CreateDeviceResources(
                graphicsDevice,
                graphicsDevice.SwapchainFramebuffer.OutputDescription,
                ColorSpaceHandling.Linear);
        }
        Off<PrepareFrameResourcesEvent>();
    }

    public void Dispose()
    {
        ImGuiRenderer?.Dispose();
        ImGuiRenderer = null;
    }

    public void Render(IRenderable renderable, CommandList cl, GraphicsDevice device, IResourceSetHolder set1, IResourceSetHolder set2)
    {
        if (cl == null) throw new ArgumentNullException(nameof(cl));
        if (device == null) throw new ArgumentNullException(nameof(device));
        if (renderable is not DebugGuiRenderable)
            throw new ArgumentException($"{GetType().Name} was passed renderable of unexpected type {renderable?.GetType().Name ?? "null"}", nameof(renderable));

        ImGuiRenderer.Render(device, cl);
        cl.SetFullScissorRects();
    }
}
