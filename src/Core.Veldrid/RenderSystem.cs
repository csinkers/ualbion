using System;
using System.Collections.Generic;
using UAlbion.Api;
using UAlbion.Api.Eventing;
using UAlbion.Core.Events;
using UAlbion.Core.Veldrid.Events;
using Veldrid;
using VeldridGen.Interfaces;

namespace UAlbion.Core.Veldrid;

public sealed class RenderSystem : Component, IRenderSystem, IDisposable
{
    public string Name { get; init; }
    internal List<RenderPass> Passes { get; init; }
    internal List<IFramebufferHolder> Framebuffers { get; init; }
    internal IResourceProvider ResourceProvider { get; init; }

    readonly StringCache<int> _stringCache = new();
    readonly PrepareDeviceObjectsEvent _prepareDeviceObjectsEvent = new();
    readonly PrepareFrameEvent _prepareFrameEvent = new();
    readonly PrepareFrameResourcesEvent _prepareFrameResourcesEvent = new();
    readonly PrepareFrameResourceSetsEvent _prepareFrameResourceSetsEvent = new();
    readonly CommandListHolder _frameCommands;
    readonly FenceHolder _fence;
    bool _addedChildren;

    CommandList FrameCommands => _frameCommands.CommandList;
    Fence Fence => _fence.Fence;

    public RenderSystem(IEnumerable<IComponent> extraComponents)
    {
        ArgumentNullException.ThrowIfNull(extraComponents);
        _frameCommands = AttachChild(new CommandListHolder("cl_main"));
        _fence = AttachChild(new FenceHolder("f_main"));

        foreach (var component in extraComponents)
            AttachChild(component);
    }

    public override string ToString() => $"RenderSystem:{Name}";

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

        _prepareDeviceObjectsEvent.Device = graphicsDevice;
        Raise(_prepareDeviceObjectsEvent);

        using (PerfTracker.FrameEvent("Prepare resources"))
        {
            FrameCommands.Begin();

            _prepareFrameResourcesEvent.Device = graphicsDevice;
            _prepareFrameResourcesEvent.CommandList = FrameCommands;
            _prepareFrameResourceSetsEvent.Device = graphicsDevice;
            _prepareFrameResourceSetsEvent.CommandList = FrameCommands;

            Raise(_prepareFrameEvent);
            Raise(_prepareFrameResourcesEvent);
            Raise(_prepareFrameResourceSetsEvent);

            FrameCommands.End();
        }

        using (PerfTracker.FrameEvent("Submit prepare commands"))
        {
            Fence.Reset();
            graphicsDevice.SubmitCommands(FrameCommands, Fence);
            graphicsDevice.WaitForFence(Fence);
        }

        int i = 0;
        foreach (var phase in Passes)
        {
            using (FrameEventCached(ref i, (this, phase), static (n, x) => $"{n} {x.Item1.Name} Render - {x.phase.Name}"))
            {
                FrameCommands.Begin();
                phase.Render(graphicsDevice, FrameCommands, ResourceProvider?.ResourceSet);
                FrameCommands.End();
            }

            Fence.Reset();
            using (FrameEventCached(ref i, (this, phase), static (n, x) => $"{n} {x.Item1.Name} Submit commands - {x.phase.Name}"))
                graphicsDevice.SubmitCommands(FrameCommands, Fence);

            using (FrameEventCached(ref i, (this, phase), static (n, x) => $"{n} {x.Item1.Name} Complete - {x.phase.Name}"))
                graphicsDevice.WaitForFence(Fence);
        }
    }

    public void Dispose()
    {
        foreach (var child in Children)
            if (child is IDisposable disposable)
                disposable.Dispose();

        RemoveAllChildren();
    }

    FrameTimeTracker FrameEventCached<T>(ref int num, T context, Func<int, T, string> builder)
        => PerfTracker.FrameEvent(_stringCache.Get(num++, context, builder));
}
