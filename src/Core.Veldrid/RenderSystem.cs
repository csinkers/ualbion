using System;
using System.Collections.Generic;
using UAlbion.Api;
using UAlbion.Api.Eventing;
using UAlbion.Core.Events;
using UAlbion.Core.Veldrid.Events;
using Veldrid;
using VeldridGen.Interfaces;

namespace UAlbion.Core.Veldrid;

public sealed class RenderSystem : Component, IRenderPipeline, IDisposable
{
    public string Name { get; init; }
    internal List<RenderPass> Passes { get; init; }
    internal List<IFramebufferHolder> Framebuffers { get; init; }
    internal IResourceProvider ResourceProvider { get; init; }

    readonly PrepareFrameEvent _prepareFrameEvent = new();
    readonly PrepareFrameResourcesEvent _prepareFrameResourcesEvent = new();
    readonly PrepareFrameResourceSetsEvent _prepareFrameResourceSetsEvent = new();
    CommandList _frameCommands;
    Fence _fence;
    bool _addedChildren;

    public RenderSystem(IEnumerable<IComponent> extraComponents)
    {
        ArgumentNullException.ThrowIfNull(extraComponents);

        foreach (var component in extraComponents)
            AttachChild(component);

        On<DeviceCreatedEvent>(e => RebuildDeviceObjects(e.Device));
        On<DestroyDeviceObjectsEvent>(_ => DestroyDeviceObjects());
    }

    public override string ToString() => $"RenderSystem:{Name}";
    void RebuildDeviceObjects(GraphicsDevice device)
    {
        if (_frameCommands != null)
            return;

        _frameCommands = device.ResourceFactory.CreateCommandList();
        _fence = device.ResourceFactory.CreateFence(false);
    }

    void DestroyDeviceObjects()
    {
        _frameCommands?.Dispose();
        _fence?.Dispose();
        _frameCommands = null;
        _fence = null;
    }

    protected override void Subscribing()
    {
        if (_addedChildren)
            return; // Already initialised?

        if (ResourceProvider is IComponent rpComponent)
            AttachChild(rpComponent);

        foreach (var framebuffer in Framebuffers)
            if (framebuffer is IComponent component)
                AttachChild(component);

        foreach (var pass in Passes)
            AttachChild(pass);

        _addedChildren = true;
    }

    public void Render(GraphicsDevice graphicsDevice)
    {
        ArgumentNullException.ThrowIfNull(graphicsDevice);

        if (!IsActive)
            throw new InvalidOperationException($"Tried to render using an inactive RenderSystem ({Name})");

        if (_frameCommands == null)
        {
            RebuildDeviceObjects(graphicsDevice);
            if (_frameCommands == null)
                return;
        }

        using (PerfTracker.FrameEvent("Prepare resources"))
        {
            _frameCommands.Begin();

            _prepareFrameResourcesEvent.Device = graphicsDevice;
            _prepareFrameResourcesEvent.CommandList = _frameCommands;
            _prepareFrameResourceSetsEvent.Device = graphicsDevice;
            _prepareFrameResourceSetsEvent.CommandList = _frameCommands;

            Raise(_prepareFrameEvent);
            Raise(_prepareFrameResourcesEvent);
            Raise(_prepareFrameResourceSetsEvent);

            _frameCommands.End();
        }

        using (PerfTracker.FrameEvent("Submit prepare commands"))
        {
            _fence.Reset();
            graphicsDevice.SubmitCommands(_frameCommands, _fence);
            graphicsDevice.WaitForFence(_fence);
        }

        int i = 0;
        foreach (var phase in Passes)
        {
            using (FrameEventCached(ref i, (this, phase), static (x, n) => $"{n} {x.Item1.Name} Render - {x.phase.Name}"))
            {
                _frameCommands.Begin();
                phase.Render(graphicsDevice, _frameCommands, ResourceProvider?.ResourceSet);
                _frameCommands.End();
            }

            _fence.Reset();
            using (FrameEventCached(ref i, (this, phase), static (x, n) => $"{n} {x.Item1.Name} Submit commands - {x.phase.Name}"))
                graphicsDevice.SubmitCommands(_frameCommands, _fence);

            using (FrameEventCached(ref i, (this, phase), static (x, n) => $"{n} {x.Item1.Name} Complete - {x.phase.Name}"))
                graphicsDevice.WaitForFence(_fence);
        }
    }

    public void Dispose()
    {
        foreach (var child in Children)
            if (child is IDisposable disposable)
                disposable.Dispose();

        RemoveAllChildren();
        DestroyDeviceObjects();
    }

    readonly List<string> _cachedStrings = new();

    FrameTimeTracker FrameEventCached<T>(ref int num, T context, Func<T, int, string> builder)
    {
        if (_cachedStrings.Count <= num)
            _cachedStrings.Add(builder(context, num));

        var message = _cachedStrings[num++];
        return PerfTracker.FrameEvent(message);
    }
}